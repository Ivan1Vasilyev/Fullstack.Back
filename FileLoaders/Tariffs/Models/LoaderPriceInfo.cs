namespace Backend.FileLoaders.Tariffs.Models
{
    public record LoaderPriceInfo(int? Price, int? PromoPrice, int? PromoLength, string? AddInfo, int? ConnectionPrice)
    {
        public int? CurrentPrice => Price.HasValue && PromoPrice.HasValue ? Math.Min(PromoPrice.Value, Price.Value) : (PromoPrice ?? Price);

        public int? BeforePrice => Price.HasValue && PromoPrice.HasValue ? Math.Max(PromoPrice.Value, Price.Value) : null;

        private string _codeCache;

        public string Code
        {
            get
            {
                if (_codeCache != null) return _codeCache;

                _codeCache = $"plata";

                if(Price.HasValue)
                {
                    _codeCache += $"-{Price}";
                }

                if (PromoPrice.HasValue)
                {
                    _codeCache += $"-promo-{PromoPrice}";
                }

                if (PromoLength.HasValue)
                {
                    _codeCache += $"-mes-{PromoLength}";
                }

                if (ConnectionPrice.HasValue)
                {
                    _codeCache += $"-podcluchenie-{ConnectionPrice}";
                }

                return _codeCache;
            }
        }

        public LoaderPriceInfo NullIfEmpty()
        {
            if (CurrentPrice.HasValue)
            {
                return this;
            }
            return null;
        }
    }
}
