using Backend.Models.Context.CityTag;

namespace Backend.Repositories.CityTags
{
    public interface ICityTagsRepository
    {
        Task<IEnumerable<CityTagModel>> GetByProviderId(int id);
    }
}