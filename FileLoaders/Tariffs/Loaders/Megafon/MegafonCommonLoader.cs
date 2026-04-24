using Backend.Databases.Postgres;
using Backend.FileLoaders.Tariffs.Models;
using Backend.Models.Context.Common;
using Backend.Utils.Excel;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Backend.FileLoaders.Tariffs.Loaders.Megafon
{
    public class MegafonCommonLoader(IPgConnectionFactory connectionFactory, IExcelHelper excelHelper) : AutoTarrifFileLoader(connectionFactory, excelHelper)
    {
        public override string LoaderName => "Мегафон файл";
        public override string TargetCode => "megafon";
        protected override LoaderProvider Provider => new(ProvidersEnum.MEGAFON);
        protected override bool UseIndexHeaderRow => false;
        protected override string DefaultCity => "Москва и область";   // Это не ошибка
        protected override string DefaultRegion => "Россия";
        protected override string[] DefaultTags => CityTagsEnum.MOSCOW_TAGS;
        protected override MegafonColumnMapping ColumnMapping => new();

        private Regex _numReg = new("(\\d+)", RegexOptions.Compiled);

        private static string GetWordEnding(int number, string one, string two, string many)
        {
            number = number % 100;
            if (number > 4 && number < 21) return many;
            number = number % 10;
            return number == 1 ? one : (number > 1 && number < 5) ? two : many;
        }

        protected override LoaderDevice? GetTvDevice(DataRow row)
        {
            var rassrochka = ColumnMapping.TvPristRassrochka36.GetValue(row)?.Replace("+", "").ParseInteger();
            var rassrochkaWink = ColumnMapping.TvPristWinkRassrochka36.GetValue(row)?.Replace("+", "").ParseInteger();
            var rassrochkaMin = new int?[] { rassrochka, rassrochkaWink }.Min();

            return new LoaderDevice("pristavka", false, null, null, null, null, rassrochkaMin).NullIfEmpty();
        }

        private string GetPriceInfo(DataRow row, int? promoLength)
        {
            //  вместо 50% и 100% приходит "0,5" и "1" соответственно
            var promoValue = ColumnMapping.PromoValue.GetValue(row);
            var isLength = promoLength.HasValue;
            var isPromoValue = !string.IsNullOrEmpty(promoValue);
            var sb = new StringBuilder();

            if (isLength && isPromoValue)
            {
                if (isPromoValue)
                {
                    promoValue = promoValue.Replace(',', '.');
                    if (double.TryParse(promoValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                    {
                        sb.Append($"-{(int)(value * 100)}%");
                    }
                }
                if (promoLength.HasValue)
                {
                    var promoLengthString = GetWordEnding(promoLength.Value, "месяц", "месяца", "месяцев");
                    sb.Append($" на {promoLength.Value} {promoLengthString}");
                }

                if (promoLength == 1 && promoValue == "1")
                {
                    sb.Append(";первый месяц бесплатно");
                }
            }

            return sb.ToString();
        }

        protected override IEnumerable<LoaderTariffOptions> GetTariffOptions(DataRow row)
        {
            var prices = ColumnMapping.MultiPrice.GetValues(row).ToDictionary(x => int.Parse(_numReg.Match(x.Key).Groups[0].Value), x => x.Value);
            var promoPrices = ColumnMapping.MultiPromoPrice.GetValues(row).ToDictionary(x => int.Parse(_numReg.Match(x.Key).Groups[0].Value), x => x.Value);
            var promoPricesLength = ColumnMapping.MegafonPromoPriceLength.GetValue(row)?.ParseInteger();
            var info = GetTariffInfo(row);
            var router = GetWiFiRouter(row)?.NullIfEmpty();
            var tv = GetTvOptions(row).NullIfEmpty();
            var mobile = GetMobileOptions(row)?.NullIfEmpty();
            var connectionPrice = ColumnMapping.ConnectionPrice.GetValue(row)?.ParseInteger();
            var priceAddInfo = GetPriceInfo(row, promoPricesLength);

            var speeds = new List<LoaderTariffOptions>();

            foreach (var price in prices)
            {
                var priceInfo = new LoaderPriceInfo(
                    price.Value?.ParseInteger(),
                    promoPrices[price.Key]?.ParseInteger(),
                    promoPricesLength,
                    priceAddInfo,
                    connectionPrice
                ).NullIfEmpty();

                if (priceInfo != null)
                {
                    speeds.Add(
                        new LoaderTariffOptions(
                            info,
                            priceInfo,
                            new LoaderInternetOptions(
                                price.Key,
                                router,
                                null,
                                InternetConnectionTehnologyEnum.Unknown
                            ),
                            tv,
                            mobile,
                            null
                       )
                    );
                }
            }

            speeds.Reverse();
            return speeds;
        }

        protected override LoaderTvOptions GetTvOptions(DataRow row)
        {
            var channelsString = ColumnMapping.MegafonChannels.GetValue(row);
            var channels = string.IsNullOrWhiteSpace(channelsString)
                ? null
                : channelsString
                    .Replace("Смотрешка ", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                    .Replace("Wink ", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                    .Replace("+", string.Empty, StringComparison.Ordinal)
                    .Trim()
                    .ParseInteger();
            var rassrochkaSber = ColumnMapping.TvPristSberRassrochka36.GetValue(row)?.Replace("+", "").ParseInteger();
            var tvDevice2 = new LoaderDevice("pristavka2", false, null, null, null, null, rassrochkaSber).NullIfEmpty();

            return new LoaderTvOptions(
                channels,
                null, null, null,
                GetTvDevice(row),
                tvDevice2
            );
        }

        protected override string GetTariffInfo(DataRow row)
        {
            var info = ColumnMapping.MegafonInfo.GetValue(row);
            if (string.IsNullOrWhiteSpace(info) || info.Contains("не доступна")) return string.Empty;

            info = info.Replace("0 сим БЕСПЛАТНО", string.Empty, StringComparison.InvariantCultureIgnoreCase).Replace("\n", ". ").Trim();

            var sb = new StringBuilder();
            sb.Append(info);

            if (info.EndsWith("БЕСПЛАТНО", StringComparison.InvariantCultureIgnoreCase))
            {
                sb.Append('!');
            }

            var familyPrice = ColumnMapping.MegafonInfoPrice.GetValue(row)?.ParseInteger();
            if (familyPrice.HasValue)
            {
                sb.Append($" Цена за дополнительные сим-карты по МегаСемье: {familyPrice} руб. ежемесячно.");
            }

            var familyAddGb = ColumnMapping.MegafonInfoAddGb.GetValue(row)?.ParseInteger();
            if (familyAddGb.HasValue)
            {
                sb.Append($" Добавьте в МегаСемью номера ваших близких в Личном кабинете, и ежемесячно вам будет начисляться дополнительный интернет — по {familyAddGb} ГБ за каждый номер, даже если вы добавите только один");
            }

            return sb.ToString();
        }

        protected override string? GetMobileInfo(DataRow row)
        {
            var minutesAdd = ColumnMapping.MegafonMinutesAdd.GetValue(row)?.ParseInteger();
            var gbAdd = ColumnMapping.MegafonGbAdd.GetValue(row)?.ParseInteger();

            var sb = new StringBuilder();

            if (minutesAdd.HasValue && minutesAdd > 0)
            {
                sb.Append($"minutes {minutesAdd}");
            }
            if (gbAdd.HasValue && gbAdd > 0)
            {
                sb.Append($";gb {gbAdd}");
            }

            return sb.ToString();
        }

        protected override LoaderTariff? GetTariff(DataRow row, Dictionary<string, HashSet<string>> indexes)
        {
            var tariffName = GetTariffName(row)?.Trim();

            if (tariffName == null)
                return null;

            var options = GetTariffOptions(row);
            var firstOption = options.FirstOrDefault();

            if (firstOption?.PriceInfo == null)
                return null;

            var price = firstOption.PriceInfo;
            var internet = firstOption.InternetOptions;
            var tv = firstOption.TvOptions;
            var mobile = firstOption.MobileOptions;

            if (internet == null && tv == null && mobile == null)
                return null;

            return new LoaderTariff(
                tariffName,
                firstOption.Info,
                price.PromoPrice != null,
                GetPriority(row),
                price,
                internet,
                tv,
                mobile,
                GetVideonabludenie(row)?.NullIfEmpty(),
                Provider,
                GetCities(row, indexes),
                options
            );
        }

        protected override string[] GetCityTags(DataRow row, string city, string region, string cityOrIndex, HashSet<string> cityList)
        {
            if (city.Contains("Москва", StringComparison.InvariantCultureIgnoreCase))
            {
                return DefaultTags;
            }

            return CityTagsEnum.RUSSIA_WITHOUT_MOSCOW_AREA_TAGS;
        }

        protected override IEnumerable<LoaderCity> GetCities(DataRow row, Dictionary<string, HashSet<string>> indexes)
        {
            var city = ReplaceYo(ColumnMapping.Region?.GetValue(row) ?? DefaultCity);
            var region = DefaultRegion;

            if (!indexes.TryGetValue(city, out var cityList))
            {
                cityList = [city];
            }

            return [.. cityList.Select(x => {
                var cityName = CheckCityName(x);
                return new LoaderCity(cityName, region, GetCityTags(row, cityName, region, city, cityList));
            })];
        }

        private string CheckCityName(string cityName)
        {
            if (cityName.Contains("Москва", StringComparison.InvariantCultureIgnoreCase))
                return "Москва и область";

            if (cityName.Contains("Санкт-Петербург", StringComparison.InvariantCultureIgnoreCase))
                return "Санкт-Петербург и область";

            return cityName;
        }
    }
}