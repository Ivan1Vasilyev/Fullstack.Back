using Backend.Models.Context.Site;
using Backend.Models.Context.Site.Contracts;
using Backend.Services.Sites;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SitesController(ISitesService sitesService) : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var sites = await sitesService.GetByProviderIdAsync(id);
            return Ok(sites);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSiteRequest request)
        {
            var site = await sitesService.CreateAsync(request);
            return Ok(site);
        }

        [HttpPatch]
        public async Task<IActionResult> Update([FromBody] UpdateSiteRequest request)
        {
            var site = await sitesService.UpdateAsync(request);
            return Ok(site);
        }
    }
}
