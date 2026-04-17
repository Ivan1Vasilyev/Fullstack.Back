using Backend.Utils;
using System.Text.RegularExpressions;

namespace Backend.FileLoaders.Tariffs.Models
{
    public partial record LoaderCity(string Name, string Region, string[] Tags, string? DomainCode = null, int? SourceCityId = null, string? CityType = null) : ILoaderCode
    {
        private string? _codeCache = DomainCode;

        [GeneratedRegex("[-]+")]
        private static partial Regex DashRegex();

        private static readonly Regex _dashRegex = DashRegex();

        public string Code
        {
            get
            {
                if (_codeCache != null) return _codeCache;

                _codeCache = $"{RegionCode}-{CityCode}";
                _codeCache = _dashRegex.Replace(_codeCache, "-");

                return _codeCache;
            }
        }

        private string? _cityCodeCache;
        public string CityCode
        {
            get
            {
                if (_cityCodeCache != null) return _cityCodeCache;
                var fullName = string.IsNullOrEmpty(CityType) ? Name : $"{CityType} {Name}";

                _cityCodeCache = LoaderCodeGenerator.GetCode(fullName);

                if(_cityCodeCache.StartsWith("g-"))
                {
                    _cityCodeCache = _cityCodeCache.Substring(2);
                }

                return _cityCodeCache;
            }
        }


        private string? _regionCodeCache;
        public string RegionCode
        {
            get
            {
                if (_regionCodeCache != null) return _regionCodeCache;

                _regionCodeCache = LoaderCodeGenerator.GetCode(Region);

                return _regionCodeCache;
            }
        }
    }
}
