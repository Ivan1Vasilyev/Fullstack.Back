using Backend.Models.Context.Page.Contracts;
using Backend.Services.Pages;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagesController(IPagesService pagesService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetByParentId([FromQuery] GetByParentIdRequest request)
        {
            var pages = await pagesService.GetByParentIdAsync(request);
            return Ok(pages);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePage([FromBody] CreatePageRequest request)
        {
            var page = await pagesService.CreateAsync(request);
            return Ok(page);
        }

        [HttpPatch]
        public async Task<IActionResult> UpdatePage([FromBody] UpdatePageRequest request)
        {
            var page = await pagesService.UpdateAsync(request);
            return Ok(page);
        }

        [HttpPatch("alias")]
        public async Task<IActionResult> UpdateAlias([FromBody] UpdatePageUrlRequest request)
        {
            var alias = await pagesService.UpdateUrlAsync(request);
            return Ok(alias);
        }
    }
}
