using Backend.Models.Context.Site;

namespace Backend.Repositories.Sites
{
    public interface ISitesRepository
    {
        Task<IEnumerable<Site>> GetByProviderIdAsync(int providerId);
        Task<Site?> CreateAsync(int providerId, string domainName, string? yandexCounterKey);
        Task<Site?> UpdateAsync(int Id, string domainName, string? yandexCounterKey);
    }
}
