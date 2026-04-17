using Backend.Databases.Postgres;
using Backend.FileLoaders.Tariffs.ColumnMappings;
using Backend.FileLoaders.Tariffs.Models;
using Backend.Utils.Excel;
using System.Data;

namespace Backend.FileLoaders.Tariffs
{
    public abstract class AutoTarrifFileLoader(IPgConnectionFactory connectionFactory, IExcelHelper excelHelper) : PgExcelFileLoader(connectionFactory, excelHelper)
    {
        protected abstract LoaderProvider Provider { get; }
        protected virtual bool UseTariffHeaderRow { get; } = true;
        protected virtual bool UseIndexHeaderRow { get; } = true;
        protected virtual int IndexHeaderRowIndex { get; } = 0;
        protected virtual int TariffHeaderRowIndex { get; } = 1;
        protected virtual string DefaultRegion { get; } = "Москва и область";
        protected virtual string DefaultCity { get; } = "Москва";
        protected virtual string[] DefaultTags { get; } = [];
        protected virtual InternetConnectionTehnologyEnum DefaultInternetConnectionTehnology { get; } = InternetConnectionTehnologyEnum.GPON;

        protected virtual ColumnMapping ColumnMapping { get; } = new CommonColumnMapping();

        protected virtual bool IsTarrifList(DataTable dataTable)
        {
            return dataTable.TableName.Trim().Equals("тарифы", StringComparison.OrdinalIgnoreCase);
        }

        protected virtual bool IsIndexList(DataTable dataTable)
        {
            return dataTable.TableName.Trim().Equals("индексы", StringComparison.OrdinalIgnoreCase);
        }

        protected virtual Dictionary<string, HashSet<string>> GetIndex(DataTable list)
        {
            var result = new Dictionary<string, HashSet<string>>();

            if (ColumnMapping.IndexName != null && ColumnMapping.IndexCity != null)
            {
                foreach (DataRow row in list.Rows)
                {
                    var indexName = ColumnMapping.IndexName.GetValue(row);
                    var indexCity = ColumnMapping.IndexCity.GetValue(row);

                    if (!string.IsNullOrWhiteSpace(indexCity) && !string.IsNullOrWhiteSpace(indexName))
                    {
                        if (result.TryGetValue(indexName, out var existing))
                        {
                            existing.Add(indexCity);
                        }
                        else
                        {
                            result.Add(indexName, [indexCity]);
                        }
                    }
                }
            }

            return result;
        }

        protected virtual Dictionary<string, HashSet<string>> GetIndex(DataSet book)
        {
            var index = new Dictionary<string, HashSet<string>>();

            foreach (DataTable list in book.Tables)
            {
                if (IsIndexList(list))
                {
                    var indexList = NormalizeHeaderRowOnIndexList(list);
                    foreach (var indexKv in GetIndex(indexList))
                    {
                        if (index.TryGetValue(indexKv.Key, out var existing))
                        {
                            foreach (var city in indexKv.Value)
                            {
                                existing.Add(city);
                            }
                        }
                        else
                        {
                            index[indexKv.Key] = indexKv.Value;
                        }
                    }
                }
            }

            return index;
        }

        protected virtual DataTable NormalizeHeaderRows(DataTable dataTable, int headerRowIndex)
        {
            var headerRow = dataTable.Rows[headerRowIndex];
            var result = new DataTable(dataTable.TableName);

            for (var i = 0; i < headerRow.Table.Columns.Count; i++)
            {
                result.Columns.Add(headerRow[i] as string, typeof(string));
            }

            foreach (var row in dataTable.Rows.Cast<DataRow>().Skip(headerRowIndex + 1))
            {
                result.Rows.Add(row.ItemArray);
            }

            return result;
        }

        protected virtual DataTable NormalizeHeaderRowOnIndexList(DataTable dataTable)
        {
            if (UseIndexHeaderRow)
            {
                return NormalizeHeaderRows(dataTable, IndexHeaderRowIndex);
            }

            return dataTable;
        }

        protected virtual DataTable NormalizeHeaderRowsOnTariffsList(DataTable dataTable)
        {
            if (UseTariffHeaderRow)
            {
                return NormalizeHeaderRows(dataTable, TariffHeaderRowIndex);
            }

            return dataTable;
        }

        protected override IEnumerable<LoaderTariff> Processing(DataSet book)
        {
            var index = GetIndex(book);
            var result = new List<LoaderTariff>();

            foreach (DataTable list in book.Tables)
            {
                if (IsTarrifList(list))
                {
                    var normalizedList = NormalizeHeaderRowsOnTariffsList(list);
                    result.AddRange(GetTarrifs(normalizedList, index));
                }
            }

            return result;
        }

        protected static string ReplaceYo(string source) => source.Replace("ё", "е", StringComparison.InvariantCultureIgnoreCase);

        protected virtual IEnumerable<LoaderCity> GetCities(DataRow row, Dictionary<string, HashSet<string>> indexes)
        {
            var cityOrIndex = ReplaceYo(ColumnMapping.City?.GetValue(row) ?? DefaultCity);
            var region = ReplaceYo(ColumnMapping.Region?.GetValue(row) ?? DefaultRegion);

            if (!indexes.TryGetValue(cityOrIndex, out var cityList))
            {
                cityList = [cityOrIndex];
            }

            return [.. cityList.Select(x => {
                var cityName = ReplaceYo(x);
                return new LoaderCity(cityName, region, GetCityTags(row, cityName, region, cityOrIndex, cityList));
            })];
        }

        protected virtual string[] GetCityTags(DataRow row, string city, string region, string cityOrIndex, HashSet<string> cityList)
        {
            return DefaultTags;
        }

        protected virtual LoaderPriceInfo GetPrice(DataRow row)
        {
            return new LoaderPriceInfo(
                ColumnMapping.Price?.GetValue(row)?.ParseInteger(),
                ColumnMapping.PromoPrice?.GetValue(row)?.ParseInteger(),
                ColumnMapping.PromoPriceLength?.GetValue(row)?.ParseInteger(),
                ColumnMapping.PriceInfo?.GetValue(row),
                ColumnMapping.ConnectionPrice?.GetValue(row)?.ParseInteger()
            );
        }

        protected virtual int GetPriority(DataRow row)
        {
            return ColumnMapping.Priority?.GetValue(row)?.ParseInteger() ?? 0;
        }

        protected virtual LoaderDevice? GetWiFiRouter(DataRow row)
        {
            return new LoaderDevice(
               "router",
               ColumnMapping.WiFiInComplect?.GetValue(row)?.ParseBoolean() ?? false,
               ColumnMapping.WiFiBuy?.GetValue(row)?.ParseInteger(),
               ColumnMapping.WiFiArenda?.GetValue(row)?.ParseInteger(),
               ColumnMapping.WiFiRassrochka?.GetValue(row)?.ParseInteger(),
               ColumnMapping.WiFiRassrochka24?.GetValue(row)?.ParseInteger(),
               ColumnMapping.WiFiRassrochka36?.GetValue(row)?.ParseInteger()
           ).NullIfEmpty();
        }

        protected virtual LoaderDevice? GetWiFiRouter2(DataRow row)
        {
            return new LoaderDevice(
                "router2",
                ColumnMapping.WiFi2InComplect?.GetValue(row)?.ParseBoolean() ?? false,
                ColumnMapping.WiFi2Buy?.GetValue(row)?.ParseInteger(),
                ColumnMapping.WiFi2Arenda?.GetValue(row)?.ParseInteger(),
                ColumnMapping.WiFi2Rassrochka?.GetValue(row)?.ParseInteger(),
                ColumnMapping.WiFi2Rassrochka24?.GetValue(row)?.ParseInteger(),
                ColumnMapping.WiFi2Rassrochka36?.GetValue(row)?.ParseInteger()
            ).NullIfEmpty();
        }

        protected virtual LoaderInternetOptions GetInternetOptions(DataRow row)
        {
            return new LoaderInternetOptions(
                ColumnMapping.InternetSpeed?.GetValue(row)?.ParseInteger(),
                GetWiFiRouter(row),
                GetWiFiRouter2(row),
                ColumnMapping.InternetConnectionTehnology?.GetValue(row)?.ParseInternetTechnology() ?? DefaultInternetConnectionTehnology
            );
        }

        protected virtual string? GetMobileInfo(DataRow row) => ColumnMapping.MobComment?.GetValue(row);

        protected virtual LoaderMobileOptions? GetMobileOptions(DataRow row)
        {
            return new LoaderMobileOptions(
                ColumnMapping.MobMin?.GetValue(row)?.ParseInteger(),
                ColumnMapping.MobSms?.GetValue(row)?.ParseInteger(),
                ColumnMapping.MobGb?.GetValue(row)?.ParseInteger(),
                GetMobileInfo(row)
            );
        }

        protected virtual LoaderVideonabludenie? GetVideonabludenie(DataRow row)
        {
            return new LoaderVideonabludenie(
                new LoaderDevice(
                    "videocamera",
                    false,
                    null,
                    ColumnMapping.VideocameraArenda?.GetValue(row)?.ParseInteger(),
                    null
                )
            ).NullIfEmpty();
        }

        protected virtual LoaderDevice? GetTvDevice(DataRow row)
        {
            return new LoaderDevice(
                "pristavka",
                ColumnMapping.TvPristInComplect?.GetValue(row)?.ParseBoolean() ?? false,
                ColumnMapping.TvPristBuy?.GetValue(row)?.ParseInteger(),
                ColumnMapping.TvPristArenda?.GetValue(row)?.ParseInteger(),
                ColumnMapping.TvPristRassrochka?.GetValue(row)?.ParseInteger(),
                ColumnMapping.TvPristRassrochka24?.GetValue(row)?.ParseInteger(),
                ColumnMapping.TvPristRassrochka36?.GetValue(row)?.ParseInteger()
            ).NullIfEmpty();
        }

        protected virtual LoaderDevice? GetTvDevice2(DataRow row)
        {
            return new LoaderDevice(
                "pristavka2",
                ColumnMapping.TvPrist2InComplect?.GetValue(row)?.ParseBoolean() ?? false,
                ColumnMapping.TvPrist2Buy?.GetValue(row)?.ParseInteger(),
                ColumnMapping.TvPrist2Arenda?.GetValue(row)?.ParseInteger(),
                ColumnMapping.TvPrist2Rassrochka?.GetValue(row)?.ParseInteger(),
                ColumnMapping.TvPrist2Rassrochka24?.GetValue(row)?.ParseInteger(),
                ColumnMapping.TvPrist2Rassrochka36?.GetValue(row)?.ParseInteger()
            ).NullIfEmpty();
        }

        protected virtual LoaderTvOptions GetTvOptions(DataRow row)
        {
            return new LoaderTvOptions(
                ColumnMapping.TvChannels?.GetValue(row)?.Replace("+", "").ParseInteger(),
                ColumnMapping.HdChannels?.GetValue(row)?.Replace("+", "").ParseInteger(),
                ColumnMapping.UhdChannels?.GetValue(row)?.Replace("+", "").ParseInteger(),
                ColumnMapping.InteractiveTv?.GetValue(row)?.Replace("+", "").ParseInteger(),
                GetTvDevice(row),
                GetTvDevice2(row)
            );
        }

        protected virtual IEnumerable<LoaderTariffOptions> GetTariffOptions(DataRow row)
        {
            return [];
        }

        protected virtual string GetTariffName(DataRow row)
        {
            return ColumnMapping.TariffName?.GetValue(row); ;
        }

        protected virtual LoaderTariff? GetTariff(DataRow row, Dictionary<string, HashSet<string>> indexes)
        {
            var tariffName = GetTariffName(row).Trim();

            if (tariffName == null)
                return null;

            var priceInfo = GetPrice(row)?.NullIfEmpty();

            if (priceInfo == null)
            {
                return null;
            }

            var internet = GetInternetOptions(row)?.NullIfEmpty();
            var tv = GetTvOptions(row)?.NullIfEmpty();
            var mobile = GetMobileOptions(row)?.NullIfEmpty();

            if (internet == null && tv == null && mobile == null)
            {
                return null;
            }

            var options = GetTariffOptions(row);

            return new LoaderTariff(
                tariffName,
                GetTariffInfo(row),
                ColumnMapping.IsAction?.GetValue(row)?.ParseBoolean() ?? false,
                GetPriority(row),
                priceInfo,
                internet,
                tv,
                mobile,
                GetVideonabludenie(row)?.NullIfEmpty(),
                Provider,
                GetCities(row, indexes),
                options.Count() > 1 ? options : []
            );
        }

        protected virtual string? GetTariffInfo(DataRow row)
        {
            return ColumnMapping.TariffInfo?.GetValue(row);
        }

        private IEnumerable<LoaderTariff> GetTarrifs(DataTable tariffsList, Dictionary<string, HashSet<string>> indexes)
        {
            var result = new LoaderTariff[tariffsList.Rows.Count];

            //long j = 0;
            //foreach (var row in tariffsList.Rows.OfType<DataRow>())
            //{
            //    result[j++] = GetTariff(row, indexes);
            //}

            Parallel.ForEach(tariffsList.Rows.OfType<DataRow>(), (row, s, i) =>
            {
                result[i] = GetTariff(row, indexes);
            });

            return [.. result.Where(x => x != null)];
        }
    }
}
