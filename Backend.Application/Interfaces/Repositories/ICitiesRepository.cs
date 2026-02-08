using Backend.Domain.Models;

namespace Backend.Application.Interfaces.Repositories
{
    public interface ICitiesRepository
    {
        Task<IEnumerable<City>> GetProviderCitiesAsync(int id);
    }
}
