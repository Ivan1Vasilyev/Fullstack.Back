using Backend.Models.Context.CityTag;

namespace Backend.Services.CityTags
{
    public interface ICityTagsService
    {
        Task<IEnumerable<CityTagModel>> GetByProviderId(int providerId);
    }
}