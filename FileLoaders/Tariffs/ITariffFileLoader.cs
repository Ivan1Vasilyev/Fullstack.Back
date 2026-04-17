using Backend.FileLoaders.Tariffs.Models;

namespace Backend.FileLoaders.Tariffs
{
    public interface ITariffFileLoader
    {
        abstract string LoaderName { get; }
        Task<IEnumerable<LoaderTariff>> LoadAsync(IFormFile file, FileLoaderOptions options);
    }
}
