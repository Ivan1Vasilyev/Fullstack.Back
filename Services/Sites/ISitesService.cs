using Backend.Models.Context.Site;
using Backend.Models.Context.Site.Contracts;

namespace Backend.Services.Sites
{
    public interface ISitesService
    {
        Task<Site> CreateAsync(CreateSiteRequest request);
        Task<IEnumerable<Site>> GetByProviderIdAsync(int providerId);
        Task<Site> UpdateAsync(UpdateSiteRequest request);
    }
}