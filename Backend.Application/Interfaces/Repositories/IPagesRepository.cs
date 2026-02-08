using Backend.Domain.Models;

namespace Backend.Application.Interfaces.Repositories
{
    public interface IPagesRepository
    {
        Task<IEnumerable<Page>> GetProviderPagesAsync(int id);
        Task<IEnumerable<Page>> GetChildPagesAsync(string request);
    }
}
