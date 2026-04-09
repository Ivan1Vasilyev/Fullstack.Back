using Backend.Models.Context.Provider;
using Backend.Models.Context.Provider.Contracts;

namespace Backend.Application.Services
{
    public interface IProvidersService
    {
        Task<Provider> CreateAsync(CreateProviderRequest request);
        Task<IEnumerable<Provider>> GetAllAsync();
        Task<Provider> GetByIdAsync(int id);
        Task<Provider> UpdateAsync(UpdateProviderRequest request);
    }
}