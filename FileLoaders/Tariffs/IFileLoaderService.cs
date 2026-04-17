using Backend.FileLoaders.Tariffs.Models;

namespace Backend.FileLoaders.Tariffs
{
    public interface IFileLoaderService
    {
        ITariffFileLoader GetLoaderByName(string loaderName);
        IEnumerable<ITariffFileLoader> GetLoaders();
        Task<IEnumerable<LoaderTariff>> LoadAsync(IFormFile file, FileLoaderOptions options);
    }
}