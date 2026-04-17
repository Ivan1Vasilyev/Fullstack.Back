using Backend.FileLoaders.Tariffs.Models;

namespace Backend.FileLoaders.Tariffs
{
    public class FileLoaderService(IServiceProvider sp) : IFileLoaderService
    {
        IServiceProvider _sp = sp;
        SemaphoreSlim _semaphore = new (1);

        public ITariffFileLoader GetLoaderByName(string loaderName)
        {
            var loader = _sp.GetServices<ITariffFileLoader>().FirstOrDefault(x => x.LoaderName == loaderName);

            return loader;
        }

        public IEnumerable<ITariffFileLoader> GetLoaders()
        {
            return _sp.GetServices<ITariffFileLoader>();
        }

        public async Task<IEnumerable<LoaderTariff>> LoadAsync(IFormFile file, FileLoaderOptions options)
        {
            await _semaphore.WaitAsync();
            try
            {
                var loader = GetLoaderByName(options.Loader);

                if (loader == null)
                {
                    throw new InvalidOperationException($"Нет загрузчика с именем \"{options.Loader}\"");
                }

                return await loader.LoadAsync(file, options);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
