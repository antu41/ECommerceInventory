using Application.DTOs;

namespace Application.Services
{
    public interface IProductService
    {
        Task<ProductDto> CreateAsync(ProductCreateDto dto, CancellationToken token, string? imagePath = null);
        Task<IEnumerable<ProductDto>> GetAllAsync(Guid? categoryId, decimal? minPrice, decimal? maxPrice, int page, int limit, CancellationToken token);
        Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken token);
        Task UpdateAsync(Guid id, ProductUpdateDto dto, CancellationToken token, string? imagePath = null);
        Task DeleteAsync(Guid id, CancellationToken token);
        Task<IEnumerable<ProductDto>> SearchAsync(string keyword, CancellationToken token);
    }
}
