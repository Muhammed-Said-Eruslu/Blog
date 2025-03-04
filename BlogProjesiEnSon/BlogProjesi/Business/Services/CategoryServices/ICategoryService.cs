using Business.DTOs.CategoryDTOs;
using Domain.Entites;
using Domain.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services.CategoryServices
{
    public interface ICategoryService
    {
        Task<IResult> AddAsync(CategoryCreateDTO categoryCreateDTO);
        Task<IDataResult<List<CategoryListDTO>>> GetAllAsync();
        Task<IResult> DeleteAsync(Guid id);
        Task<IDataResult<CategoryDTO>> GetByIdAsync(Guid id);
        Task<IResult> UpdateAsync(CategoryUpdateDTO categoryUpdateDTO);
        Task<IDataResult<CategoryDTO>> GetBySlugAsync(string slug);
        Task<IResult> BulkDeleteAsync(List<Guid> ids);
        void RemoveRange(IEnumerable<Category> entities);
    }
}

