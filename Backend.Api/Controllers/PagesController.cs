using Backend.Application.Contracts.Pages;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/create")]
    public class PagesController(IPagesService pageService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreatePage([FromBody] CreatePageRequest request)
        {
            var page = await pageService.CreateAsync(request);
            return Ok(page);
        }
    }
}
