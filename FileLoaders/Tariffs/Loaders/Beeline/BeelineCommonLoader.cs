using Backend.Databases.Postgres;
using Backend.FileLoaders.Tariffs.Models;
using Backend.Utils.Excel;
using System.Data;

namespace Backend.FileLoaders.Tariffs.Loaders.Beeline
{
    public class BeelineCommonLoader(IPgConnectionFactory connectionFactory, IExcelHelper excelHelper) : AutoTarrifFileLoader(connectionFactory, excelHelper)
    {
        public override string LoaderName => "Билайн (наш файл)";
        public override string TargetCode => "beeline";
        protected override LoaderProvider Provider => new LoaderProvider(ProvidersEnum.BEELINE);
        protected override string DefaultCity => "Москва";
        protected override string DefaultRegion => "Московская область";
        private Dictionary<string, string> _RegionByCityIndex = [];

        protected override IEnumerable<LoaderCity> GetCities(DataRow row, Dictionary<string, HashSet<string>> indexes)
        {
            var cityIndex = ColumnMapping.City?.GetValue(row) ?? DefaultCity;
            var region = DefaultRegion;
            if (_RegionByCityIndex.TryGetValue(cityIndex, out var regionName))
            {
                region = regionName;
            }

            if (!indexes.TryGetValue(cityIndex, out var cityList))
            {
                cityList = [cityIndex];
            }

            return cityList.Select(x => new LoaderCity(x, region, GetCityTags(row, x, region, cityIndex, cityList))).ToArray();
        }

        protected override Dictionary<string, HashSet<string>> GetIndex(DataTable list)
        {
            var result = new Dictionary<string, HashSet<string>>();

            if (ColumnMapping.IndexName != null && ColumnMapping.IndexCity != null)
            {
                foreach (DataRow row in list.Rows)
                {
                    var indexName = ColumnMapping.IndexName.GetValue(row);
                    var indexCity = ColumnMapping.IndexCity.GetValue(row);
                    var region = ColumnMapping.Region?.GetValue(row) ?? DefaultRegion;

                    if (!string.IsNullOrWhiteSpace(indexCity) && !string.IsNullOrWhiteSpace(indexName))
                    {
                        _RegionByCityIndex[indexName] = region;
                        if (result.TryGetValue(indexName, out var existing))
                        {
                            existing.Add(indexCity);
                        }
                        else
                        {
                            result.Add(indexName, [ indexCity ]);
                        }
                    }
                }
            }

            return result;
        }

        protected override string[] GetCityTags(DataRow row, string city, string region, string cityOrIndex, HashSet<string> cityList) =>
            region switch
            {
                "Москва" => CityTagsEnum.MOSCOW_TAGS,
                "Московская область" => CityTagsEnum.MOSCOW_AREA_TAGS,
                _ => CityTagsEnum.RUSSIA_WITHOUT_MOSCOW_AREA_TAGS
            };
    }
}
