using Microsoft.AspNetCore.Mvc;
using FloodRelief.Services;

namespace FloodRelief.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly UploadService _service;

        public UploadController(UploadService service)
        {
            _service = service;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(
        IFormFile file
        )
        {
            return await _service.UploadImage(file);
        }
    }
}
