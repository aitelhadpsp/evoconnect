using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvoConnect.Common;
using EvoConnect.Common.Models;
using EvoConnect.Server.Repository;
using EvoConnect.Server.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EvoConnect.Server.Controllers
{
    public class FilesController(IPartnerDA partnerDA) : Controller
    {
        [HttpGet("/imagsse/{*path}")]
        public IActionResult GetImage(string path)
        {
            var fullPath = Path.Combine(AppData.ImageDir(), path);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var ext = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
            var contentType = $"image/{ext}";

            // This tells ASP.NET Core to stream the file directly
            return PhysicalFile(fullPath, contentType, enableRangeProcessing: true);
        }

        [HttpDelete("/image/{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            try
            {
                var dbImage = await partnerDA.GetImage(id);

                if (dbImage == null)
                {
                    return NotFound(new { success = false, message = "Image not found" });
                }

                var fullPath = Path.Combine(AppData.ImageDir(), dbImage.PatientId.ToString(), dbImage.Path);

                // Delete from database
                await partnerDA.DeleteImage(id);

                // Delete physical file
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("/image/{id:int}")]
        public async Task<IActionResult> GetIamgeById(int id)
        {
            var dbImage = await partnerDA.GetImage(id);

            var fullPath = Path.Combine(AppData.ImageDir(), dbImage.Path);

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            return Ok(dbImage);
        }

        [HttpGet("/device.xml")]
        public IActionResult DeviceXml()
        {
            var xmlContent = @"<?xml version=""1.0""?>
                <root xmlns=""urn:schemas-upnp-org:device-1-0"">
                    <specVersion>
                        <major>1</major>
                        <minor>0</minor>
                    </specVersion>
                    <device>
                        <deviceType>urn:schemas-upnp-org:device:Basic:1</deviceType>
                        <friendlyName>DentalEvo Connect Server</friendlyName>
                        <manufacturer>Meriva</manufacturer>
                        <modelName>DentalEvo UPnP Server</modelName>
                        <UDN>uuid:b0baad54-3bd0-4394-be2d-35cb53ff8e50</UDN>
                        <presentationURL>http://localhost:6222/</presentationURL>
                    </device>
                </root>";

            return Content(xmlContent, "application/xml");
        }

        [HttpPost("/image/{id}")]
        public async Task<IActionResult> UpdateImage(int id, IFormFile image)
        {
            var name = DateTime.Now.Ticks.ToString();
            var ext = image.FileName.Split(".").Last();

            var dbImage = await partnerDA.GetImage(id);

            var oldPath = Path.Combine(AppData.ImageDir(), dbImage.Path);
            var filePath = Path.Combine(AppData.ImageDir(), dbImage.PatientId.ToString(), name + "." + ext);

            using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            await partnerDA.UpdateImagePath(dbImage.Id, name, ext);

            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            return Ok(new
            {
                success = true
            });
        }

        [HttpPost("/toggle-image-label")]
        public async Task<IActionResult> TogglePatientImageLabel([FromBody] ToggleImageLabelRequest input)
        {
            var result = await partnerDA.TogglePatientImageLabel(input.ImageId, input.LabelId);
            if (!result)
            {
                return BadRequest(new { success = false, message = "Failed to toggle label" });
            }
            return Ok(new { success = true });
        }

        [HttpPost("/add-images/{id}")]
        public async Task<IActionResult> AddImage(int id, List<IFormFile> images)
        {
            if (images == null || images.Count == 0)
            {
                return BadRequest(new { success = false, message = "No images provided" });
            }

            var patient = await partnerDA.GetPatient(id);
            if (patient == null)
            {
                return NotFound(new { success = false, message = "Patient not found" });
            }

            var uploadedImages = new List<object>();

            foreach (var image in images)
            {
                var name = DateTime.Now.Ticks.ToString();
                var ext = Path.GetExtension(image.FileName).TrimStart('.');

                var dbImage = await partnerDA.AddImage(patient.InternId, name, ext);

                // Ensure directory exists
                var directory = Path.Combine(AppData.ImageDir(), dbImage.PatientId.ToString());
                Directory.CreateDirectory(directory);

                var filePath = Path.Combine(directory, name + "." + ext);

                // Actually save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                uploadedImages.Add(new { id = dbImage.Id, path = filePath });

            }

            return Ok(new
            {
                success = true,
                images = uploadedImages
            });
        }
    }
    public class ToggleImageLabelRequest
    {
        public int ImageId { get; set; }
        public int LabelId { get; set; }
    }
}