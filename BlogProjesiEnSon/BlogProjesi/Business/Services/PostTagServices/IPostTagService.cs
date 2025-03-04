using Business.DTOs.PostDTOs;
using Business.DTOs.PostTagDTOs;
using Business.DTOs.TagDTOs;
using Domain.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services.PostTagServices
{
    public interface IPostTagService
    {
        Task<IResult> AddAsync(PostTagCreateDTO postTagCreateDTO);
        Task<IDataResult<PostTagDTO>> GetByIdAsync(Guid id);
        Task<IDataResult<List<PostTagDTO>>> GetAllAsync();
        Task<IResult> UpdateAsync(PostTagUpdateDTO postTagUpdateDTO);
        Task<IResult> DeleteAsync(Guid id);
        Task<IDataResult<List<TagDTO>>> GetTagsByPostIdAsync(Guid postId);
        Task<IDataResult<List<PostDTO>>> GetPostsByTagIdAsync(Guid tagId);
        Task<IResult> AddTagsToPostAsync(Guid postId, List<Guid> tagIds);
        Task<IResult> RemoveTagsFromPostAsync(Guid postId, List<Guid> tagIds);
        Task<IDataResult<object>> CheckIfTagExistsInPostAsync(Guid postId, Guid tagId);
        Task<IResult> TogglePostTagActivationAsync(Guid postTagId);
        Task<IResult> RemoveAllTagsFromPostAsync(Guid postId);

    }
}
