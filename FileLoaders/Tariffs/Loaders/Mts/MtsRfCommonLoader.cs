using Backend.Databases.Postgres;
using Backend.FileLoaders.Tariffs.Models;
using Backend.Utils.Excel;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace Backend.FileLoaders.Tariffs.Loaders.Mts
{
    public class MtsRfCommonLoader(IPgConnectionFactory connectionFactory, IExcelHelper excelHelper) : AutoTarrifFileLoader(connectionFactory, excelHelper)
    {
        protected override MtsColumnMapping ColumnMapping => new();

        //Используем первую строку каждого листа как имя колонки
        protected override int TariffHeaderRowIndex => 0;
        protected override bool UseTariffHeaderRow => true;

        //Не сипользуем страничку с индексами
        protected override bool UseIndexHeaderRow => false;

        public override string LoaderName => "МТС - Россия";
        public override string TargetCode => "mts-russia";

        protected override LoaderProvider Provider => new (ProvidersEnum.MTS);

        private readonly Regex _numReg = new ("(\\d+)", RegexOptions.Compiled);

        protected override string[] GetCityTags(DataRow row, string city, string region, string cityOrIndex, HashSet<string> cityList)
        {
            return CityTagsEnum.RUSSIA_WITHOUT_MOSCOW_AREA_TAGS;
        }

        protected override bool IsIndexList(DataTable dataTable) => false;

        protected override bool IsTarrifList(DataTable dataTable) => true;

        private static string CustomRoundFraction(string value)
        {
            var index = value.IndexOf('.');
            if (index < 0)
            {
                index = value.IndexOf(',');
            }
            if (index >= 0)
            {
                return value[..index];
            }

            return value;
        }

        protected override int GetPriority(DataRow row)
        {
            var tariffName = GetTariffName(row) ?? "";
            if (tariffName.Contains("Выгодно", StringComparison.OrdinalIgnoreCase)) return 5;
            if (tariffName.Contains("РИИЛ", StringComparison.OrdinalIgnoreCase)) return 4;
            if (tariffName.Contains("Хорошо", StringComparison.OrdinalIgnoreCase)) return 3;
            if (tariffName.Contains("Супер", StringComparison.OrdinalIgnoreCase)) return 2;
            if (tariffName.Contains("Отлично", StringComparison.OrdinalIgnoreCase)) return 1;
            return 0;
        }

        protected override LoaderDevice? GetWiFiRouter(DataRow row)
        {
            var wiFiInComplect = ColumnMapping.WiFiInComplect?.GetValue(row)?.ParseBoolean() ?? false;
            var wiFiBuy = ColumnMapping.WiFiBuy?.GetValue(row)?.ParseInteger();
            var wiFiArendaValue = ColumnMapping.WiFiArenda?.GetValue(row);
            if (!string.IsNullOrEmpty(wiFiArendaValue))
            {
                wiFiArendaValue = CustomRoundFraction(wiFiArendaValue);
            }
            var wiFiArenda = wiFiArendaValue?.ParseInteger();
            var wiFiRassrochka = ColumnMapping.WiFiRassrochka?.GetValue(row)?.ParseInteger();
            var wiFiRassrochka36 = ColumnMapping.WiFiRassrochka36?.GetValue(row)?.ParseInteger();

            return new LoaderDevice("router", wiFiInComplect, wiFiBuy, wiFiArenda == 1 ? null : wiFiArenda, wiFiRassrochka, null, wiFiRassrochka36).NullIfEmpty();
        }

        private string? GetFirstMonth(DataRow row)
        {
            var isFirstMonth = ColumnMapping.MonthForFree?.GetValue(row)?.ParseBoolean();
            return isFirstMonth == true ? "Месяц в подарок" : null;

        }

        protected override LoaderPriceInfo GetPrice(DataRow row)
        {
            var price = ColumnMapping.Price?.GetValue(row)?.ParseInteger();
            var promoPrice = ColumnMapping.PromoPrice?.GetValue(row)?.ParseInteger();
            var promoLength = ColumnMapping.PromoPriceLength?.GetValue(row)?.ParseInteger();
            var connectionCost = ColumnMapping.ConnectionPrice?.GetValue(row)?.ParseInteger();
            //  месяцы в подарок
            var addInfo = GetFirstMonth(row);

            return new LoaderPriceInfo(price, promoPrice, promoLength, addInfo, connectionCost);
        }

        protected override LoaderDevice? GetTvDevice(DataRow row)
        {
            var tvPristInComplect = ColumnMapping.TvPristInComplect?.GetValue(row)?.ParseBoolean() ?? false;
            var tvPristBuy = ColumnMapping.TvPristBuy?.GetValue(row)?.ParseInteger() ?? ColumnMapping.TvHdPristBuyPrice.GetValue(row)?.ParseInteger();
            var tvPristRassrochka = ColumnMapping.TvPristRassrochka.GetValue(row)?.ParseInteger();
            var tvPristRassrochka36 = ColumnMapping.TvPristRassrochka36.GetValue(row)?.ParseInteger();

            var tvHdPristArendaValue = ColumnMapping.TvHdPristArenda?.GetValue(row);

            if (!string.IsNullOrEmpty(tvHdPristArendaValue))
            {
                tvHdPristArendaValue = CustomRoundFraction(tvHdPristArendaValue);
            }
            var tvPristArenda = ColumnMapping.TvPristArenda?.GetValue(row)?.ParseInteger();
            var tvPristIptvArenda = ColumnMapping.TvIptvPristArenda.GetValue(row)?.ParseInteger();
            var tvHdPristArenda = tvHdPristArendaValue?.ParseInteger();
            var resultTvPristArenda = new int?[] { tvPristArenda, tvHdPristArenda, tvPristIptvArenda }.Min();

            return new LoaderDevice("pristavka", tvPristInComplect, tvPristBuy, resultTvPristArenda, tvPristRassrochka, null, tvPristRassrochka36).NullIfEmpty();
        }

        private static readonly Regex _spaces = new Regex("\\s+", RegexOptions.Compiled);
        protected override string GetTariffInfo(DataRow row)
        {
            var sb = new StringBuilder();

            var baseInfo = base.GetTariffInfo(row);
            if (!string.IsNullOrEmpty(baseInfo))
                sb.AppendLine(baseInfo);

            var notes = ColumnMapping.RussiaNotes?.GetValue(row);
            if (!string.IsNullOrEmpty(notes))
                sb.AppendLine(notes);

            var comments = ColumnMapping.RussiaComments?.GetValue(row);
            if (!string.IsNullOrEmpty(comments))
                sb.AppendLine(comments);

            var groupInfoPrice = ColumnMapping.GroupForFamiliars?.GetValue(row);
            var groupInfo = string.IsNullOrWhiteSpace(groupInfoPrice)
                ? string.Empty
                : $"Стоимость за каждого акцептора {groupInfoPrice} рублей. Всего можно подключить до 5 участников, у которых будет тариф без абонентской платы. \r\nВладелец оплачивает группу для близких в день её создания.";
            if (!string.IsNullOrEmpty(groupInfo))
                sb.AppendLine(groupInfo);

            return _spaces.Replace(sb.ToString(), " ").Trim();
        }

        protected override IEnumerable<LoaderTariffOptions> GetTariffOptions(DataRow row)
        {
            var prices = ColumnMapping.MultiPrice.GetValues(row).ToDictionary(x => int.Parse(_numReg.Match(x.Key).Groups[0].Value), x => x.Value);
            var optionPrices = ColumnMapping.MultiOptionPrice.GetValues(row).ToDictionary(x => int.Parse(_numReg.Match(x.Key).Groups[0].Value), x => x.Value);
            var promoPrice = ColumnMapping.PromoPrice.GetValue(row)?.ParseInteger();
            var promoLength = ColumnMapping.PromoPriceLength?.GetValue(row)?.ParseInteger();
            var firstPrice = ColumnMapping.Price?.GetValue(row)?.ParseInteger();
            var speeds = new List<LoaderTariffOptions>();
            var addInfo = GetFirstMonth(row);

            foreach (var price in prices)
            {
                var optionPriceValue = price.Value?.ParseInteger();
                var optionDifference = optionPrices[price.Key];
                if (optionPriceValue == null && optionDifference == null) continue;

                var isPromoPriceNull = promoPrice == null;

                var calcedPrice = firstPrice + (optionDifference?.ParseInteger());
                var optionPrice = isPromoPriceNull
                    ? optionPriceValue ?? calcedPrice
                    : calcedPrice;

                var optionPromoPrice = isPromoPriceNull
                    ? null
                    : optionPriceValue ?? promoPrice + (optionDifference?.ParseInteger());

                var priceInfo = new LoaderPriceInfo(
                    optionPrice,
                    optionPromoPrice,
                    promoLength,
                    addInfo,
                    null
                ).NullIfEmpty();

                var key = price.Key == 1 ? 1000 : price.Key;

                if (priceInfo != null)
                {
                    speeds.Add(
                        new LoaderTariffOptions(
                            null,
                            priceInfo,
                            new LoaderInternetOptions(
                                key,
                                null,
                                null,
                                InternetConnectionTehnologyEnum.Unknown
                            ),
                            null,
                            null,
                            null
                       )
                    );
                }
            }
            if (speeds.Count > 0)
            {
                var firstSpeed = ColumnMapping.InternetSpeed?.GetValue(row)?.ParseInteger();
                var firstPriceInfo = new LoaderPriceInfo(
                    firstPrice,
                    promoPrice,
                    promoLength,
                    addInfo,
                    null
                );

                var firstOption = new LoaderTariffOptions(
                    null,
                    firstPriceInfo,
                    new LoaderInternetOptions(
                        firstSpeed,
                        null,
                        null,
                        InternetConnectionTehnologyEnum.Unknown
                    ),
                    null,
                    null,
                    null
                );
                speeds.Insert(0, firstOption);
            }

            return speeds.Count > 1 ? speeds : [];
        }
    }
}
