using Backend.Application.Services;
using Backend.Models.Context.Provider.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProvidersController(IProvidersService providerService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var providers = await providerService.GetAllAsync();
            return Ok(providers);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await providerService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPatch]
        public async Task<IActionResult> Update([FromBody] UpdateProviderRequest request)
        {
            var result = await providerService.UpdateAsync(request);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProviderRequest request)
        {
            var result = await providerService.CreateAsync(request);
            return Ok(result);
        }
    }
}
