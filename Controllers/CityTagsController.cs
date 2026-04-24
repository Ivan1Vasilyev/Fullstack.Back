using Backend.Services.CityTags;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CityTagsController(ICityTagsService cityTagsService) : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var cityTags = await cityTagsService.GetByProviderId(id);
            return Ok(cityTags);
        }
    }
}
