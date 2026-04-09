using Backend.Models.Context.Page;
using Backend.Models.Context.Page.Contracts;

namespace Backend.Services.Pages
{
    public interface IPagesService
    {
        Task<IEnumerable<Page>> GetByParentIdAsync(GetByParentIdRequest request);
        Task<Page> CreateAsync(CreatePageRequest request);
        Task<Page> UpdateAsync(UpdatePageRequest request);
        Task<string> UpdateUrlAsync(UpdatePageUrlRequest request);
    }
}