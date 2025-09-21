using Application.DTOs;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;


namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductDto> CreateAsync(ProductCreateDto dto, CancellationToken token, string? imagePath = null)
        {
            try
            {
                var product = new Product
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    Stock = dto.Stock,
                    CategoryId = dto.CategoryId,
                    ImagePath = string.IsNullOrWhiteSpace(imagePath)
                        ? string.Empty
                        : imagePath
                };

                await _unitOfWork.Products.AddAsync(product, token);
                await _unitOfWork.SaveChangesAsync(token);

                return await MapToDto(product, token);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync(Guid? categoryId, decimal? minPrice, decimal? maxPrice, int page, int limit, CancellationToken token)
        {
            try
            {
                var query = _unitOfWork.Products.GetAll()
                              .Include(p => p.Category)
                              .AsNoTracking();

                if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);
                if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice);
                if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice);

                var products = await query.Skip((page - 1) * limit).Take(limit).ToListAsync(token);

                return products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? string.Empty,
                    ImagePath = p.ImagePath
                });
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken token)
        {
            try
            {
                var product = await _unitOfWork.Products.GetAll()
                     .Include(p => p.Category)
                     .AsNoTracking()
                     .FirstOrDefaultAsync(p => p.Id == id, token);

                if (product == null) return null;

                return new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category?.Name ?? string.Empty,
                    ImagePath = product.ImagePath
                };
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task UpdateAsync(Guid id, ProductUpdateDto dto, CancellationToken token, string? imagePath = null)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id, token);
                if (product == null) throw new Exception("Product not found");

                if (dto.Name != null) product.Name = dto.Name;
                if (dto.Description != null) product.Description = dto.Description;
                if (dto.Price.HasValue) product.Price = dto.Price.Value;
                if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
                if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;
                if (imagePath != null) product.ImagePath = imagePath;

                _unitOfWork.Products.Update(product);
                await _unitOfWork.SaveChangesAsync(token);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task DeleteAsync(Guid id, CancellationToken token)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id, token);
                if (product == null) throw new Exception("Product not found");

                _unitOfWork.Products.Delete(product);
                await _unitOfWork.SaveChangesAsync(token);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<IEnumerable<ProductDto>> SearchAsync(string keyword, CancellationToken token)
        {
            try
            {
                var keywordLower = keyword.ToLower();

                var products = await _unitOfWork.Products.GetAll()
                    .Include(p => p.Category)
                    .AsNoTracking()
                    .Where(p => p.Name.ToLower().Contains(keywordLower)
                             || p.Description.ToLower().Contains(keywordLower))
                    .ToListAsync(token);


                return products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? string.Empty,
                    ImagePath = p.ImagePath
                });
            }
            catch (Exception)
            {
                throw;
            }

        }

        private async Task<ProductDto> MapToDto(Product product, CancellationToken token)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId, token);
                return new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    CategoryId = product.CategoryId,
                    CategoryName = category?.Name ?? string.Empty,
                    ImagePath = product.ImagePath
                };
            }
            catch (Exception)
            {
                throw;

            }
        }
    }
}