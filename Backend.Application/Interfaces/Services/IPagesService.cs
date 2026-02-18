using Backend.Application.Contracts.Pages;

namespace Backend.Application.Interfaces.Services
{
    public interface IPagesService
    {
        Task<IEnumerable<PageDto>> GetSitePagesAsync(int siteId);
        Task<IEnumerable<PageDto>> GetChildPagesAsync(int parentId);
        Task<PageDto> CreateAsync(CreatePageRequest page);
    }
}
