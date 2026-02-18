using Backend.Application.Contracts.Provider;

namespace Backend.Application.Interfaces.Services
{
    public interface IProvidersService
    {
        Task<IEnumerable<ProviderDto>> GetAllAsync();
        Task<ProviderDto> GetByIdAsync(int id);
        Task<ProviderDto> UpdateAsync(UpdateProviderRequest request);
        Task<ProviderDto> CreateAsync(CreateProviderRequest request);
    }
}
