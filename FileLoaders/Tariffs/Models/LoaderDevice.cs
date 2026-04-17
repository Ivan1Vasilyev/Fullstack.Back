namespace Backend.FileLoaders.Tariffs.Models
{
    public record LoaderDevice(
        string Type, 
        bool InComplect, 
        int? BuyPrice, 
        int? ArendaPrice, 
        int? RassrochkaPrice, 
        int? Rassrochka24Price = null, 
        int? Rassrochka36Price = null
    ) : ILoaderCode
    {
        private string _codeCache;

        private string GetCode()
        {
            if (_codeCache == null)
            {
                _codeCache = Type.ToLower();

                if (InComplect)
                {
                    _codeCache += $"{(InComplect ? "-compl" : "")}";
                }
                
                if(BuyPrice.HasValue)
                {
                    _codeCache += $"-{BuyPrice}";
                }

                if (ArendaPrice.HasValue)
                {
                    _codeCache += $"-{ArendaPrice}";
                }

                if (RassrochkaPrice.HasValue)
                {
                    _codeCache += $"-{RassrochkaPrice}";
                }

                if (Rassrochka36Price.HasValue)
                {
                    _codeCache += $"-{Rassrochka24Price}";
                }

                if (Rassrochka36Price.HasValue)
                {
                    _codeCache += $"-{Rassrochka36Price}";
                }
            }

            return _codeCache;
        }

        public string Code => GetCode();

        public LoaderDevice? NullIfEmpty()
        {
            if (InComplect || BuyPrice.HasValue || ArendaPrice.HasValue || RassrochkaPrice.HasValue || Rassrochka24Price.HasValue || Rassrochka36Price.HasValue)
            {
                return this;
            }

            return null;
        }
    };
}
