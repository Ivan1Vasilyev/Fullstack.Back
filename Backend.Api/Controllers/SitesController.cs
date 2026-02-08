using Backend.Application.Contracts.Sites;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SitesController(ISitesService sitesService) : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSites(int id)
        {
            var sites = await sitesService.GetByProviderIdAsync(id);
            return Ok(sites);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSite([FromBody] CreateSiteRequest request)
        {
            var site = await sitesService.CreateAsync(request);
            return Ok(site);
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateSite([FromBody] UpdateSiteRequest request)
        {
            var site = await sitesService.UpdateAsync(request);
            return Ok(site);
        }
    }
}
