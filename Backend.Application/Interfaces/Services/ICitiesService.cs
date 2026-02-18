using Backend.Application.Contracts.Cities;

namespace Backend.Application.Interfaces.Services
{
    public interface ICitiesService
    {
        Task<IEnumerable<CityDto>> GetProviderCitiesAsync(int providerId);
    }
}
