namespace Backend.FileLoaders.Tariffs.Models
{
    public record LoaderVideonabludenie(LoaderDevice? Videocamera): ILoaderCode
    {
        private string _codeCache;

        public string Code => GetCode();

        private string GetCode()
        {
            if (_codeCache == null)
            {
                _codeCache = $"{Videocamera?.Code}";
            }

            return _codeCache;
        }
        public LoaderVideonabludenie NullIfEmpty()
        {
            if (Videocamera?.NullIfEmpty() is null)
            {
                return null;
            }

            return this;
        }
    }
}
