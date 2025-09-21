using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(Policy = "ApiAccess")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IHostEnvironment _environment;

    public ProductsController(IProductService productService, IHostEnvironment environment)
    {
        _productService = productService;
        _environment = environment;
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <remarks>
    /// Sample request (multipart/form-data):
    /// - name: "Laptop"
    /// - description: "High-performance laptop"
    /// - price: 999.99
    /// - stock: 50
    /// - categoryId: 1
    /// - image: (file upload, e.g., laptop.jpg)
    /// </remarks>
    /// <param name="dto">Product creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid input or image format</response>
    /// <response code="403">Unauthorized access</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromForm] ProductCreateDto dto, CancellationToken cancellationToken)
    {
        try
        {
            string? imagePath = null;

            if (dto.Image != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                var extension = Path.GetExtension(dto.Image.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                    return BadRequest(new { error = "Only photo formats are allowed (jpg, jpeg, png, gif, bmp, webp)." });

                if (!dto.Image.ContentType.StartsWith("image/"))
                    return BadRequest(new { error = "Invalid file type. Only image files are allowed." });

                const long maxFileSize = 2 * 1024 * 1024; // 2 MB
                if (dto.Image.Length > maxFileSize)
                    return BadRequest(new { error = "File size must be less than 2 MB." });

                var uploads = Path.Combine(_environment.ContentRootPath, "images");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploads, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream, cancellationToken);
                }

                imagePath = $"/images/{uniqueFileName}";
            }

            var product = await _productService.CreateAsync(dto, cancellationToken, imagePath);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Invalid category"))
                return BadRequest(new { error = ex.Message });
            throw;
        }
    }
    /// <summary>
    /// Get all products with optional filters
    /// </summary>
    /// <remarks>
    /// Sample request: GET /api/products?categoryId=1&amp;minPrice=10&amp;maxPrice=1000&amp;page=1&amp;limit=10
    /// </remarks>
    /// <param name="categoryId">Optional category ID filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="minPrice">Optional minimum price filter</param>
    /// <param name="maxPrice">Optional maximum price filter</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="limit">Items per page (default: 10)</param>
    /// <response code="200">List of products</response>
    /// <response code="403">Unauthorized access</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? categoryId,
        CancellationToken cancellationToken,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var products = await _productService.GetAllAsync(categoryId, minPrice, maxPrice, page, limit, cancellationToken);
        return Ok(products);
    }



    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Product details</response>
    /// <response code="403">Unauthorized access</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id, cancellationToken);
            if (product == null) return NotFound(new { error = "Product not found" });
            return Ok(product);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Product not found"))
                return NotFound(new { error = ex.Message });
            throw;
        }
    }

    /// <summary>
    /// Update product
    /// </summary>
    /// <remarks>
    /// Sample request (multipart/form-data):
    /// - name: "Updated Laptop"
    /// - description: "Updated high-performance laptop"
    /// - price: 1099.99
    /// - stock: 40
    /// - categoryId: 1
    /// - image: (file upload, e.g., updated_laptop.jpg)
    /// </remarks>
    /// <param name="id">Product ID</param>
    /// <param name="dto">Product update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Product updated successfully</response>
    /// <response code="400">Invalid input or image format</response>
    /// <response code="403">Unauthorized access</response>
    /// <response code="404">Product not found</response>
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromForm] ProductUpdateDto dto, CancellationToken cancellationToken)
    {
        try
        {
            string? imagePath = null;

            if (dto.Image != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                var extension = Path.GetExtension(dto.Image.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                    return BadRequest(new { error = "Only photo formats are allowed (jpg, jpeg, png, gif, bmp, webp)." });

                if (!dto.Image.ContentType.StartsWith("image/"))
                    return BadRequest(new { error = "Invalid file type. Only image files are allowed." });

                const long maxFileSize = 2 * 1024 * 1024; // 2 MB
                if (dto.Image.Length > maxFileSize)
                    return BadRequest(new { error = "File size must be less than 2 MB." });

                var uploads = Path.Combine(_environment.ContentRootPath, "images");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploads, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream, cancellationToken);
                }

                imagePath = $"/images/{uniqueFileName}";
            }

            await _productService.UpdateAsync(id, dto, cancellationToken, imagePath);
            var product = await _productService.GetByIdAsync(id, cancellationToken);
            return Ok(product);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Product not found"))
                return NotFound(new { error = ex.Message });
            if (ex.Message.Contains("Invalid category"))
                return BadRequest(new { error = ex.Message });
            throw;
        }
    }

    /// <summary>
    /// Delete product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">Product deleted successfully</response>
    /// <response code="403">Unauthorized access</response>
    /// <response code="404">Product not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id, cancellationToken);
            if (product == null)
                return NotFound(new { error = "Product not found" });

            if (!string.IsNullOrEmpty(product.ImagePath))
            {
                var fullPath = Path.Combine(_environment.ContentRootPath, product.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            await _productService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Product not found"))
                return NotFound(new { error = ex.Message });
            throw;
        }
    }

    /// <summary>
    /// Search products by name or description
    /// </summary>
    /// <remarks>
    /// Sample request: GET /api/products/search?q=laptop
    /// </remarks>
    /// <param name="q">Search keyword</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">List of matching products</response>
    /// <response code="403">Unauthorized access</response>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken cancellationToken)
    {
        try
        {
            var products = await _productService.SearchAsync(q, cancellationToken);
            return Ok(products);
        }
        catch (Exception)
        {
            throw;
        }
    }
}