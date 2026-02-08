using Backend.Application.DTOs.Cities;
using Backend.Application.Exceptions;
using Backend.Application.Interfaces.Repositories;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Models;

namespace Backend.Application.Services
{
    public class CitiesService(ICitiesRepository citiesRepository) : ICitiesService
    {
        public async Task<IEnumerable<CityDto>> GetProviderCitiesAsync(int providerId)
        {
            try
            {
                var cities = await citiesRepository.GetProviderCitiesAsync(providerId);
                return cities.Select(MapToCityDto);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        private CityDto MapToCityDto(City city) => new(city.Id, city.ProviderId, city.CityName, city.DomainCode);
    }
}
