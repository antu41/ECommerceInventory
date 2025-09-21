using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize(Policy = "ApiAccess")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "name": "Electronics",
    ///   "description": "Gadgets and devices"
    /// }
    /// </remarks>
    /// <param name="dto">Category creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="201">Category created successfully</response>
    /// <response code="403">Unauthorized access</response>
    /// <response code="409">Category name already exists</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Category name already exists"))
            {
                return Conflict(new { error = ex.Message });
            }
            throw;
        }
    }

    /// <summary>
    /// Get all categories with product counts
    /// </summary>
    /// <response code="200">List of categories</response>
    /// <response code="403">Unauthorized access</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _categoryService.GetAllAsync(cancellationToken);
            return Ok(categories);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Category details</response>
    /// <response code="403">Unauthorized access</response>
    /// <response code="404">Category not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id, cancellationToken);
            if (category == null) return NotFound(new { error = "Category not found" });
            return Ok(category);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Category not found"))
            {
                return NotFound(new { error = ex.Message });
            }
            throw;
        }
    }

    /// <summary>
    /// Update category
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "name": "Updated Electronics",
    ///   "description": "Updated gadgets"
    /// }
    /// </remarks>
    /// <param name="id">Category ID</param>
    /// <param name="dto">Category update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Category updated successfully</response>
    /// <response code="403">Unauthorized access</response>
    /// <response code="404">Category not found</response>
    /// <response code="409">Category name already exists</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryUpdateDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _categoryService.UpdateAsync(id, dto, cancellationToken);
            var category = await _categoryService.GetByIdAsync(id, cancellationToken);
            return Ok(category);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Category not found"))
            {
                return NotFound(new { error = ex.Message });
            }
            if (ex.Message.Contains("Category name already exists"))
            {
                return Conflict(new { error = ex.Message });
            }
            throw;
        }
    }

    /// <summary>
    /// Delete category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">Category deleted successfully</response>
    /// <response code="403">Unauthorized access</response>
    /// <response code="404">Category not found</response>
    /// <response code="409">Category has linked products</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _categoryService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Category not found"))
            {
                return NotFound(new { error = ex.Message });
            }
            if (ex.Message.Contains("Category has linked products"))
            {
                return Conflict(new { error = ex.Message });
            }
            throw;
        }
    }
}