using Application.DTOs;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto, CancellationToken token)
        {
            try
            {
                var category = new Category
                {
                    Name = dto.Name,
                    Description = dto.Description
                };

                await _unitOfWork.Categories.AddAsync(category, token);
                await _unitOfWork.SaveChangesAsync(token);

                return new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ProductCount = 0
                };
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAll()
                    .Include(c => c.Products)
                    .AsNoTracking()
                    .ToListAsync();

                return categories.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count
                });
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken token)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetAll()
                                .Include(c => c.Products)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(c => c.Id == id, token);

                if (category == null) return null;

                return new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ProductCount = category.Products.Count
                };
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task UpdateAsync(Guid id, CategoryUpdateDto dto, CancellationToken token)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id, token);
                if (category == null) throw new Exception("Category not found");

                if (dto.Name != null) category.Name = dto.Name;
                if (dto.Description != null) category.Description = dto.Description;

                _unitOfWork.Categories.Update(category);
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
                var category = await _unitOfWork.Categories.GetAll()
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id, token);

                if (category == null) throw new Exception("Category not found");
                if (category.Products.Any()) throw new Exception("Category has linked products");

                _unitOfWork.Categories.Delete(category);
                await _unitOfWork.SaveChangesAsync(token);
            }
            catch (Exception)
            {
                throw;

            }
        }
    }
}