using Backend.Application.Contracts.Sites;

namespace Backend.Application.Interfaces.Services
{
    public interface ISitesService
    {
        Task<IEnumerable<SiteDto>> GetByProviderIdAsync(int providerId);

        Task<SiteDto> CreateAsync(CreateSiteRequest request);
        Task<SiteDto> UpdateAsync(UpdateSiteRequest request);
    }
}
