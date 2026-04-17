using Backend.Utils;

namespace Backend.FileLoaders.Tariffs.Models
{
    public record LoaderProvider(string Name): ILoaderCode
    {
        private string _codeCache;
        public string Code
        {
            get
            {
                if (_codeCache != null) return _codeCache;

                _codeCache = LoaderCodeGenerator.GetCode(Name);

                return _codeCache;
            }
        }
    }
}
