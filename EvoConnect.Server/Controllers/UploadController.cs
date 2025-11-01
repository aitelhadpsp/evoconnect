using EvoConnect.Common;

using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace FileUploadApp.Controllers
{
    public class UploadController : Controller
    {
        private readonly string _storagePath;

        public UploadController()
        {
            // Set the storage path for uploaded files
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {


                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                var filePath = Path.Combine(AppData.UploadDir(), file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return Ok(new { success = true });
            }
            catch (Exception ex) {


                return Ok(new { success = true });

            }
        }
    }
}
