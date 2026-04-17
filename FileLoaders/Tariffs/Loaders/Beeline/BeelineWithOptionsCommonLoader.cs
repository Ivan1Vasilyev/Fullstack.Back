using Backend.Databases.Postgres;
using Backend.FileLoaders.Tariffs.Models;
using Backend.Utils.Excel;
using System.Data;
using System.Text.RegularExpressions;

namespace Backend.FileLoaders.Tariffs.Loaders.Beeline
{
    public class BeelineWithOptionsCommonLoader(IPgConnectionFactory connectionFactory, IExcelHelper excelHelper) : BeelineCommonLoader(connectionFactory, excelHelper)
    {
        public override string LoaderName => "Билайн с опциями (наш файл)";

        protected override BeelineColumnMapping ColumnMapping => new();
        private readonly Regex _numReg = new Regex("(\\d+)", RegexOptions.Compiled);

        protected override IEnumerable<LoaderTariffOptions> GetTariffOptions(DataRow row)
        {
            var prices = ColumnMapping.MultiPrice.GetValues(row).ToDictionary(x => int.Parse(_numReg.Match(x.Key).Groups[0].Value), x => x.Value);
            var promoPrices = ColumnMapping.MultiPromoPrice.GetValues(row).ToDictionary(x => int.Parse(_numReg.Match(x.Key).Groups[0].Value), x => x.Value);
            var promoPricesLength = ColumnMapping.MultiPromoPriceLength.GetValues(row).ToDictionary(x => int.Parse(_numReg.Match(x.Key).Groups[0].Value), x => x.Value);
            var info = ColumnMapping.PriceInfo?.GetValue(row);
            var connectionCost = ColumnMapping.ConnectionPrice?.GetValue(row)?.ParseInteger();
            var router = GetWiFiRouter(row);
            var router2 = GetWiFiRouter2(row);

            var speeds = new List<LoaderTariffOptions>();

            foreach (var price in prices)
            {
                var priceInfo = new LoaderPriceInfo(
                    price.Value?.ParseInteger(),
                    promoPrices[price.Key]?.ParseInteger(),
                    promoPricesLength[price.Key]?.ParseInteger(),
                    info,
                    connectionCost
                ).NullIfEmpty();

                if (priceInfo != null)
                {
                    speeds.Add(
                        new LoaderTariffOptions(
                            null,
                            priceInfo,
                            new LoaderInternetOptions(
                                price.Key,
                                router,
                                router2,
                                InternetConnectionTehnologyEnum.Unknown                                
                            ),
                            null,
                            null,
                            null
                       )
                    );
                }
            }

            return speeds;
        }

        protected override string? GetMobileInfo(DataRow row)
        {
            var minutesAdd = ColumnMapping.MinutesAdd.GetValue(row)?.ParseInteger();
            var gbAdd = ColumnMapping.GbAdd.GetValue(row)?.ParseInteger();
            var simBuy = ColumnMapping.NewSimBuyPrice.GetValue(row)?.ParseInteger();

            var minutesString = minutesAdd.HasValue && minutesAdd > 0
                ? $"minutes {minutesAdd}"
                : null;

            var gbAddString = gbAdd.HasValue && gbAdd > 0
               ? $"gbAdd {gbAdd}"
               : null;

            var simBuyString = simBuy.HasValue && simBuy > 0
               ? $"sim {simBuy}"
               : null;

            var result = string.Join(
                ';', 
                new string?[] { minutesString, gbAddString, simBuyString }.Where(x => !string.IsNullOrWhiteSpace(x))
            );

            return result;
        }

        protected override LoaderTariff? GetTariff(DataRow row, Dictionary<string, HashSet<string>> indexes)
        {
            var tariffName = GetTariffName(row).Trim();

            if (tariffName == null)
                return null;

            var options = GetTariffOptions(row);

            var firstOption = options.FirstOrDefault();

            if (firstOption?.PriceInfo == null)
                return null;

            var internet = firstOption.InternetOptions.NullIfEmpty();
            var tv = GetTvOptions(row)?.NullIfEmpty();
            var mobile = GetMobileOptions(row)?.NullIfEmpty();

            if (internet == null && tv == null && mobile == null)
            {
                return null;
            }

            return new LoaderTariff(
                tariffName,
                GetTariffInfo(row),
                ColumnMapping.IsAction?.GetValue(row)?.ParseBoolean() ?? false,
                GetPriority(row),
                firstOption.PriceInfo,
                internet,
                tv,
                mobile,
                GetVideonabludenie(row)?.NullIfEmpty(),
                Provider,
                GetCities(row, indexes),
                options
            );
        }
    }
}
