using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitiesController(ICitiesService citiesService) : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCities(int id)
        {
            var cities = await citiesService.GetProviderCitiesAsync(id);
            return Ok(cities);
        }
    }
}
