using Backend.Models.Context.Provider;

namespace Backend.Repositories.Providers
{
    public interface IProvidersRepository
    {
        Task<Provider?> CreateAsync(string name, string code);
        Task<IEnumerable<Provider>> GetAllAsync();
        Task<Provider?> GetByIdAsync(int id);
        Task<Provider?> UpdateAsync(int id, string newName, string newCode);
    }
}