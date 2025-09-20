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
        private readonly IWebHostEnvironment _environment;

        public ProductsController(IProductService productService, IWebHostEnvironment environment)
        {
            _productService = productService;
            _environment = environment;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto, CancellationToken token, IFormFile? image)
        {
            try
            {
                string? imagePath = null;
                if (image != null)
                {
                    var uploads = Path.Combine(_environment.WebRootPath, "images");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                    var filePath = Path.Combine(uploads, image.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    imagePath = $"/images/{image.FileName}";
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
        public async Task<IActionResult> Update(Guid id, [FromForm] ProductUpdateDto dto, CancellationToken token, IFormFile? image)
        {
            try
            {
                string? imagePath = null;
                if (image != null)
                {
                    var uploads = Path.Combine(_environment.WebRootPath, "images");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                    var filePath = Path.Combine(uploads, image.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    imagePath = $"/images/{image.FileName}";
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
