using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IHostEnvironment _environment;

        public ProductsController(IProductService productService, IHostEnvironment environment)
        {
            _productService = productService;
            _environment = environment;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto, CancellationToken token, IFormFile? image)
        {
            try
            {
                string? imagePath = null;

                if (image != null)
                {
                    // Allowed extensions
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                    var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                    if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                        return BadRequest("Only photo formats are allowed (jpg, jpeg, png, gif, bmp, webp).");

                    // Optional: check MIME type
                    if (!image.ContentType.StartsWith("image/"))
                        return BadRequest("Invalid file type. Only image files are allowed.");

                    // Optional: check file size (max 2 MB here)
                    const long maxFileSize = 2 * 1024 * 1024;
                    if (image.Length > maxFileSize)
                        return BadRequest("File size must be less than 2 MB.");

                    // Save file with unique name
                    var uploads = Path.Combine(_environment.ContentRootPath, "images");
                    if (!Directory.Exists(uploads))
                        Directory.CreateDirectory(uploads);

                    // Add GUID to filename
                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploads, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    // Store relative path
                    imagePath = $"/images/{uniqueFileName}";
                }

                var product = await _productService.CreateAsync(dto, token, imagePath);
                return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId, CancellationToken token, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var products = await _productService.GetAllAsync(categoryId, minPrice, maxPrice, page, limit, token);
                return Ok(products);
            }
            catch (Exception)
            {
                throw;
            }

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken token)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id, token);
                if (product == null) return NotFound();
                return Ok(product);
            }
            catch (Exception)
            {
                throw;
            }

        }


        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(Guid id, [FromForm] ProductUpdateDto dto, CancellationToken token, IFormFile? image)
        {
            try
            {
                string? imagePath = null;

                if (image != null)
                {
                    // Allowed extensions
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                    var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                    if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                        return BadRequest("Only photo formats are allowed (jpg, jpeg, png, gif, bmp, webp).");

                    // Optional: check MIME type
                    if (!image.ContentType.StartsWith("image/"))
                        return BadRequest("Invalid file type. Only image files are allowed.");

                    // Optional: check file size (max 2 MB)
                    const long maxFileSize = 2 * 1024 * 1024;
                    if (image.Length > maxFileSize)
                        return BadRequest("File size must be less than 2 MB.");

                    // Save file with unique name
                    var uploads = Path.Combine(_environment.ContentRootPath, "images");
                    if (!Directory.Exists(uploads))
                        Directory.CreateDirectory(uploads);

                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploads, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    imagePath = $"/images/{uniqueFileName}";
                }

                await _productService.UpdateAsync(id, dto, token, imagePath);
                return NoContent();
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken token)
        {
            try
            {
                // 1. Get product details (including image path) before deleting
                var product = await _productService.GetByIdAsync(id, token);
                if (product == null)
                    return NotFound();

                // 2. Delete image file from directory if exists
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    var fullPath = Path.Combine(_environment.ContentRootPath, product.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                // 3. Delete product record from DB
                await _productService.DeleteAsync(id, token);

                return NoContent();
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, CancellationToken token)
        {
            try
            {
                var products = await _productService.SearchAsync(q, token);
                return Ok(products);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
