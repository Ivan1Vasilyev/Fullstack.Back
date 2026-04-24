using Backend.FileLoaders;
using Backend.FileLoaders.Tariffs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileLoadersController(IFileLoaderService fileLoaderService) : ControllerBase
    {
        [HttpGet]
        public IActionResult GetTariffLoaders()
        {
            var loaders = fileLoaderService.GetLoaders();
            return Ok(loaders);
        }

        [HttpPost]
        public async Task<IActionResult> LoadAsync(IFormFile file, [FromForm] FileLoaderOptions options)
        {
            var result = await fileLoaderService.LoadAsync(file, options);
            return Ok(result);
        }
    }
}
