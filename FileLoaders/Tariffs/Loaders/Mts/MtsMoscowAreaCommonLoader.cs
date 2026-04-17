using Backend.Databases.Postgres;
using Backend.FileLoaders.Tariffs.Models;
using Backend.Utils.Excel;
using System.Data;
using System.Text.RegularExpressions;

namespace Backend.FileLoaders.Tariffs.Loaders.Mts
{
    public class MtsMoscowAreaCommonLoader(IPgConnectionFactory connectionFactory, IExcelHelper excelHelper) : AutoTarrifFileLoader(connectionFactory, excelHelper)
    {
        protected override MtsColumnMapping ColumnMapping => new();

        public override string LoaderName => "МТС - Москва и область (наш файл)";
        public override string TargetCode => "mts-moscow";

        protected override LoaderProvider Provider => new LoaderProvider(ProvidersEnum.MTS);

        protected override string DefaultCity => "Москва";
        protected override string DefaultRegion => "Московская область";

        private Regex _numReg = new Regex("(\\d+)", RegexOptions.Compiled);

        protected override LoaderPriceInfo GetPrice(DataRow row)
        {
            return GetBestOption(row)?.PriceInfo;
        }

        private LoaderTariffOptions GetBestOption(DataRow row)
        {
            var options = GetTariffOptions(row);

            var minSpeedOption = options?.OrderBy(x => x?.InternetOptions?.InternetSpeed ?? int.MaxValue).FirstOrDefault();

            return minSpeedOption;
        }

        protected override string[] GetCityTags(DataRow row, string city, string region, string cityOrIndex, HashSet<string> cityList)
        {
            if (cityOrIndex.Equals("Москва") || cityOrIndex.Equals("Москва и Новая Москва"))
            {
                return CityTagsEnum.MOSCOW_TAGS;
            }
            else
            {
                return CityTagsEnum.MOSCOW_AREA_TAGS;
            }
        }

        protected override LoaderInternetOptions GetInternetOptions(DataRow row)
        {
            return GetBestOption(row)?.InternetOptions;
        }

        protected override IEnumerable<LoaderTariffOptions> GetTariffOptions(DataRow row)
        {
            var prices = ColumnMapping.MultiPrice.GetValues(row).ToDictionary(x => int.Parse(_numReg.Match(x.Key).Groups[0].Value), x => x.Value);
            var promoPrices = ColumnMapping.MultiPromoPrice.GetValues(row).ToDictionary(x => int.Parse(_numReg.Match(x.Key).Groups[0].Value), x => x.Value);
            var promoPricesLength = ColumnMapping.MultiPromoPriceLength.GetValues(row).ToDictionary(x => int.Parse(_numReg.Match(x.Key).Groups[0].Value), x => x.Value);
            var info = ColumnMapping.PriceInfo?.GetValue(row);
            var connectionCost = ColumnMapping.ConnectionPrice?.GetValue(row)?.ParseInteger();
            var router = GetWiFiRouter(row);
            var ontRouter = GetWiFiRouter2(row);
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
                                ontRouter,
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
    }
}
