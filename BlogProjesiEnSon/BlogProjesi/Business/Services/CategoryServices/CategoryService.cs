using Business.DTOs.CategoryDTOs;
using Business.DTOs.PostDTOs;
using Domain.Entites;
using Domain.Enums;
using Domain.Utilities.Concretes;
using Domain.Utilities.Interfaces;
using Infrastructure.Repositories.CategoryRepository;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IResult> AddAsync(CategoryCreateDTO categoryCreateDTO)
        {
            if (await _categoryRepository.AnyAsync(x => x.Name.ToLower() == categoryCreateDTO.Name.ToLower()))
                return new ErrorResult("Category already exists.");

            try
            {
                var newCategory = categoryCreateDTO.Adapt<Category>();
                await _categoryRepository.AddAsync(newCategory);
                await _categoryRepository.SaveChangeAsync();

                return new SuccessResult("Category added successfully.");
            }
            catch (Exception ex)
            {
                return new ErrorResult(ex.Message);
            }
        }

        public async Task<IResult> DeleteAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return new ErrorResult("Category not found.");

            if (category.Posts?.Any(p => p.Status != Domain.Enums.Status.Deleted) ?? false)
                return new ErrorResult("Cannot delete category with active posts.");

            try
            {
                await _categoryRepository.DeleteAsync(category);
                await _categoryRepository.SaveChangeAsync();
                return new SuccessResult("Category deleted successfully.");
            }
            catch (Exception ex)
            {
                return new ErrorResult("Error: " + ex.Message);
            }
        }

        public async Task<IDataResult<List<CategoryListDTO>>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var categoryListDTOs = categories.Select(c => new CategoryListDTO
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                PostCount = c.Posts?.Count() ?? 0
            }).ToList();

            if (!categoryListDTOs.Any())
                return new ErrorDataResult<List<CategoryListDTO>>(categoryListDTOs, "No categories found.");

            return new SuccessDataResult<List<CategoryListDTO>>(categoryListDTOs, "Categories retrieved successfully.");
        }

        public async Task<IDataResult<CategoryDTO>> GetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return new ErrorDataResult<CategoryDTO>(null, "Category not found.");

            var categoryDTO = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                Posts = category.Posts?.Select(p => new PostDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    // Map other Post properties as needed
                }).ToList() ?? new List<PostDTO>()
            };

            return new SuccessDataResult<CategoryDTO>(categoryDTO, "Category retrieved successfully.");
        }

        public async Task<IResult> UpdateAsync(CategoryUpdateDTO categoryUpdateDTO)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(categoryUpdateDTO.Id);
            if (existingCategory == null)
                return new ErrorResult("Category not found.");

            if (await _categoryRepository.AnyAsync(c => c.Name.ToLower() == categoryUpdateDTO.Name.ToLower() && c.Id != categoryUpdateDTO.Id))
                return new ErrorResult("Category name already exists.");

            existingCategory.Name = categoryUpdateDTO.Name;
            existingCategory.Slug = categoryUpdateDTO.Slug;
            existingCategory.Description = categoryUpdateDTO.Description;

            try
            {
                await _categoryRepository.UpdateAsync(existingCategory);
                await _categoryRepository.SaveChangeAsync();
                return new SuccessResult("Category updated successfully.");
            }
            catch (Exception ex)
            {
                return new ErrorResult("Error: " + ex.Message);
            }
        }

        public async Task<IDataResult<CategoryDTO>> GetBySlugAsync(string slug)
        {
            var category = await _categoryRepository.GetAsync(c => c.Slug == slug);
            if (category == null)
                return new ErrorDataResult<CategoryDTO>(null, "Category not found.");

            var categoryDTO = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                Posts = category.Posts?.Select(p => new PostDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                }).ToList() ?? new List<PostDTO>()
            };

            return new SuccessDataResult<CategoryDTO>(categoryDTO, "Category retrieved by slug successfully.");
        }

        public async Task<IResult> BulkDeleteAsync(List<Guid> ids)
        {
            var categoriesToDelete = await _categoryRepository.GetAllAsync(c => ids.Contains(c.Id));
            if (!categoriesToDelete.Any())
                return new ErrorResult("No categories found to delete.");

            foreach (var category in categoriesToDelete)
            {
                // Status.Deleted kontrolü ekleyin
                if (category.Posts?.Any(p => p.Status != Status.Deleted) ?? false)
                    return new ErrorResult($"Category {category.Name} has active posts and cannot be deleted.");
            }

            try
            {
                _categoryRepository.RemoveRange(categoriesToDelete);
                await _categoryRepository.SaveChangeAsync(); // SaveChangesAsync olabilir
                return new SuccessResult("Categories deleted successfully.");
            }
            catch (Exception ex)
            {
                return new ErrorResult("Error: " + ex.Message);
            }
        }

        public void RemoveRange(IEnumerable<Category> entities)
        {
            _categoryRepository.RemoveRange(entities);
        }
    }
}