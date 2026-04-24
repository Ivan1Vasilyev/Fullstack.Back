using Backend.Models.Context.Phone;
using Backend.Models.Context.Phone.Contracts;
using Backend.Services.Phones;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhonesController(IPhonesService phonesService) : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var phones = await phonesService.GetBySiteIdAsync(id);
            return Ok(phones);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePhoneRequest request)
        {
            var phone = await phonesService.CreateAsync(request);
            return Ok(phone);
        }

        [HttpPatch]
        public async Task<IActionResult> Update([FromBody] Phone request)
        {
            var phone = await phonesService.UpdateAsync(request);
            return Ok(phone);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await phonesService.DeleteAsync(id);
            return Ok(result);
        }
    }
}
