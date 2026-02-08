using Backend.Application.Contracts.Provider;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProvidersController(IProvidersService providerService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetProviders()
        {
            var providers = await providerService.GetAllAsync();
            return Ok(providers);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProviderById(int id)
        {
            var result = await providerService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateProvider([FromBody] UpdateProviderRequest request)
        {
            var result = await providerService.UpdateAsync(request);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProvider([FromBody] CreateProviderRequest request)
        {
            var result = await providerService.CreateAsync(request);
            return Ok(result);
        }
    }
}
