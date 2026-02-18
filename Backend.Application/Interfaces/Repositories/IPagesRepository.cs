using Backend.Domain.Models;

namespace Backend.Application.Interfaces.Repositories
{
    public interface IPagesRepository
    {
        Task<IEnumerable<Page>> GetSitePagesAsync(int siteId);
        Task<IEnumerable<Page>> GetChildPagesAsync(int parentId);
        Task<Page?> CreateAsync(string name, string type, string alias, int siteId, int? parentId, string? content, string? title, string? description);
    }
}
