using Backend.Domain.Models;

namespace Backend.Application.Interfaces.Repositories
{
    public interface ISitesRepository
    {
        Task<IEnumerable<Site>> GetByProviderIdAsync(int providerId);
        Task<Site?> CreateAsync(int providerId, string domainName, string? yandexCounterKey);
        Task<Site?> UpdateAsync(int Id, string domainName, string? yandexCounterKey);
    }
}
