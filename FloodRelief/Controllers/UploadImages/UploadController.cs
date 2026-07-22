using Microsoft.AspNetCore.Mvc;

namespace FloodRelief.Controllers
{
    [Route("api/uploads")]
    [ApiController]
    public class UploadController : ControllerBase
    {

        private readonly IWebHostEnvironment _environment;


        public UploadController(
            IWebHostEnvironment environment
        )
        {
            _environment = environment;
        }



        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(
            IFormFile file
        )
        {

            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "ไม่พบไฟล์"
                });
            }



            var uploadFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads"
                );



            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }



            var fileName =
                Guid.NewGuid()
                .ToString()
                +
                Path.GetExtension(file.FileName);



            var filePath =
                Path.Combine(
                    uploadFolder,
                    fileName
                );



            using (var stream = new FileStream(
                filePath,
                FileMode.Create
            ))
            {
                await file.CopyToAsync(stream);
            }



            var imageUrl =
                $"/uploads/{fileName}";



            return Ok(new
            {
                imageUrl
            });
        }
    }
}