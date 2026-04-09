using Backend.Models.Context.Page;
using Backend.Models.Context.Page.Contracts;

namespace Backend.Repositories.Pages
{
    public interface IPagesRepository
    {
        Task<Page?> CreateAsync(CreatePageRequest request);
        Task<IEnumerable<Page>> GetByParentIdAsync(GetByParentIdRequest request);
        Task<bool> PageExistsAsync(int siteId, int? parentId, string url);
        Task<Page?> UpdateAsync(UpdatePageRequest request);
        Task<string?> UpdateAliasAsync(UpdatePageUrlRequest request);
    }
}