using Business.DTOs.TagDTOs;
using Domain.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services.TagServices
{
    public interface ITagService
    {
        //CRUD
        Task<IResult> AddAsync(TagCreateDTO tagCreateDTO);
        Task<IDataResult<TagDTO>> GetByIdAsync(Guid id);
        Task<IDataResult<List<TagDTO>>> GetAllAsync();
        Task<IResult> UpdateAsync(TagUpdateDTO tagUpdateDTO);
        Task<IResult> DeleteAsync(Guid id);

        // İşlevsel Metotlar
        Task<IDataResult<List<TagDTO>>> GetTagsByPostIdAsync(Guid postId);
        Task<IDataResult<List<TagDTO>>> GetPopularTagsAsync(int count);
        Task<IDataResult<List<TagDTO>>> SearchTagsAsync(string searchTerm);
        Task<IResult> ToggleTagActivationAsync(Guid tagId);
    }
}
