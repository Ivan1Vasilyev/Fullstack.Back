using Backend.Databases.Postgres;
using Backend.FileLoaders.Tariffs.Models;
using Backend.Models.Context.Common;
using Backend.Utils.Excel;
using System.Data;
using System.Text.RegularExpressions;

namespace Backend.FileLoaders.Tariffs.Loaders.Rostelecom
{
    public class RostelecomCommonLoader(IPgConnectionFactory connectionFactory, IExcelHelper excelHelper) : AutoTarrifFileLoader(connectionFactory, excelHelper)
    {
        protected override int TariffHeaderRowIndex => 0;
        public override string LoaderName => "Ростелеком (наш файл)";
        public override string TargetCode => "rostelecom";
        protected override LoaderProvider Provider => new LoaderProvider(ProvidersEnum.ROSTELECOM);
        protected override string DefaultCity => "Москва";
        protected override string DefaultRegion => "Московская область";
        protected override string[] DefaultTags => [];

        protected override string[] GetCityTags(DataRow row, string city, string region, string cityOrIndex, HashSet<string> cityList)
        {
            if (region == "Москва город")
            {
                return CityTagsEnum.MOSCOW_TAGS;
            }
            else if (region == "Московская область")
            {
                return CityTagsEnum.MOSCOW_AREA_TAGS;
            }
            else
            {
                return CityTagsEnum.RUSSIA_WITHOUT_MOSCOW_AREA_TAGS;
            }
        }

        private string GetCityType(string cityName) => CityTypeRegex.Match(cityName).Value.Trim();


        private readonly Regex CityTypeRegex = new("^((г)|(пгт)|(п)|(рп)|(с)|(д)|(с\\/с)|(с\\/а)|(сл)|(массив)|(р-н))\\.\\s+", RegexOptions.Compiled);

        protected override IEnumerable<LoaderCity> GetCities(DataRow row, Dictionary<string, HashSet<string>> indexes)
        {
            var cityOrIndex = ReplaceYo(ColumnMapping.City?.GetValue(row) ?? DefaultCity);
            var region = ReplaceYo(ColumnMapping.Region?.GetValue(row) ?? DefaultRegion);

            if (!indexes.TryGetValue(cityOrIndex, out var cityList))
            {
                cityList = [cityOrIndex];
            }

            return [.. cityList.Select(x => {
                var cityName = ReplaceYo(x);
                var cityType = GetCityType(cityName);

                if (!string.IsNullOrEmpty(cityType))
                    cityName = cityName.Replace(cityType, "").Trim();

                return new LoaderCity(cityName, region, GetCityTags(row, cityName, region, cityOrIndex, cityList), null, null, cityType);
            })];
        }
    }
}
