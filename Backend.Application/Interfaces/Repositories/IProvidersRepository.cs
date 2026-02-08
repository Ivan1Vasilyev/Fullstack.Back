using Backend.Domain.Models;

namespace Backend.Application.Interfaces.Repositories
{
    public interface IProvidersRepository
    {
        Task<IEnumerable<Provider>> GetAllAsync();
        Task<Provider> GetByIdAsync(int id);
        Task<Provider?> UpdateAsync(int id, string newName, string newCode);
        Task<Provider?> CreateAsync(string name, string code);
    }
}
