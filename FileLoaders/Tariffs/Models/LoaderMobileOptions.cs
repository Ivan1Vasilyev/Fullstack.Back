namespace Backend.FileLoaders.Tariffs.Models
{
    public record LoaderMobileOptions(int? Minutes, int? Sms, int? Gb, string? Info) : ILoaderCode
    {
        private string _codeCache;

        private string GetCode()
        {
            if (_codeCache == null)
            {
                _codeCache = $"mobile";

                if(Minutes.HasValue)
                {
                    _codeCache += $"-{Minutes}";
                }

                if (Sms.HasValue)
                {
                    _codeCache += $"-{Sms}";
                }

                if (Gb.HasValue)
                {
                    _codeCache += $"-{Gb}";
                }
            }
            return _codeCache;
        }

        public string Code => GetCode();

        public LoaderMobileOptions? NullIfEmpty()
        {
            if (Minutes.HasValue || Sms.HasValue || Gb.HasValue)
            {
                return this;
            }
            return null;
        }
    };
}
