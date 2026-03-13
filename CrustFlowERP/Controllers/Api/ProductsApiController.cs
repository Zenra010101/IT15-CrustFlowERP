using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CrustFlowERP.Services;
using System;
using System.Threading.Tasks;

namespace CrustFlowERP.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsApiController : ControllerBase
    {
        private readonly IStorageService _storageService;

        public ProductsApiController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        /// <summary>
        /// Uploads an image to local storage using the storage service.
        /// </summary>
        /// <param name="imageFile">The image file from the request.</param>
        /// <returns>A JSON object containing the URL of the uploaded image.</returns>
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new { success = false, message = "No image file provided." });
            }

            try
            {
                // Upload via the storage service
                string imageUrl = await _storageService.UploadFileAsync(imageFile, "products");

                if (string.IsNullOrEmpty(imageUrl))
                {
                    return StatusCode(500, new { success = false, message = "Failed to upload image to storage." });
                }

                return Ok(new { success = true, imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error uploading image: {ex.Message}" });
            }
        }
    }
}
