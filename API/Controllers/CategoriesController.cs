using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto, CancellationToken token)
        {
            try
            {
                var category = await _categoryService.CreateAsync(dto, token);
                return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
            }
            catch (Exception)
            {
                throw;
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                return Ok(categories);
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
                var category = await _categoryService.GetByIdAsync(id, token);
                if (category == null) return NotFound();
                return Ok(category);
            }
            catch (Exception)
            {
                throw;
            }

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CategoryUpdateDto dto, CancellationToken token)
        {
            try
            {
                await _categoryService.UpdateAsync(id, dto, token);
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
                await _categoryService.DeleteAsync(id, token);
                return NoContent();
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
