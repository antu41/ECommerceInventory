using Application.DTOs;

namespace Application.Services
{
    public interface ICategoryService
    {
        Task<CategoryDto> CreateAsync(CategoryCreateDto dto, CancellationToken token);
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken token);
        Task UpdateAsync(Guid id, CategoryUpdateDto dto, CancellationToken token);
        Task DeleteAsync(Guid id, CancellationToken token);
    }
}
