namespace Backend.FileLoaders.Tariffs.Models
{
    public record LoaderInternetOptions(
        int? InternetSpeed, 
        LoaderDevice? WiFiRouter, 
        LoaderDevice? WiFiRouter2,
        InternetConnectionTehnologyEnum Technology
    ) : ILoaderCode
    {
        string _codeCache;

        private string GetCode()
        {
            if (_codeCache == null)
            {
                _codeCache = $"internet-{InternetSpeed}";

                if (Technology != InternetConnectionTehnologyEnum.Unknown)
                {
                    _codeCache += $"-{Technology.ToString().ToLower()}";
                }

                if (WiFiRouter != null && WiFiRouter.InComplect)
                {
                    _codeCache += $"-{WiFiRouter?.Code}";
                }
            }

            return _codeCache;
        }

        public string Code => GetCode();

        public LoaderInternetOptions NullIfEmpty()
        {
            if (InternetSpeed.HasValue)
            {
                return this;
            }

            return null;
        }
    }
}
