using Backend.Exceptions;
using Backend.Models.Context.CityTag;
using Backend.Repositories.CityTags;

namespace Backend.Services.CityTags
{
    public class CityTagsService(ICityTagsRepository cityTagsRepository) : ICityTagsService
    {
        public async Task<IEnumerable<CityTagModel>> GetByProviderId(int providerId)
        {
            if (providerId < 1)
                throw new ValidationException("providerId должен быть больше 0");

            try
            {
                return await cityTagsRepository.GetByProviderId(providerId);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }
    }
}
