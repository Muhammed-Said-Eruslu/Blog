using Business.DTOs.TagDTOs;
using Domain.Entites;
using Domain.Utilities.Concretes;
using Domain.Utilities.Interfaces;
using Infrastructure.Repositories.TagRepository;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Services.TagServices
{
    public class TagService : ITagService
    {
        private readonly ITagRepostiry _tagRepository;

        public TagService(ITagRepostiry tagRepostiry)
        {
            _tagRepository = tagRepostiry;
        }

        public async Task<IResult> AddAsync(TagCreateDTO tagCreateDTO)
        {
            var tag = tagCreateDTO.Adapt<Tag>();
            tag.CreatedAt = DateTime.UtcNow;

            await _tagRepository.AddAsync(tag);
            await _tagRepository.SaveChangeAsync();

            return new SuccessResult("Tag added successfully.");
        }

        public async Task<IResult> DeleteAsync(Guid id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
                return new ErrorResult("Tag not found.");

            await _tagRepository.DeleteAsync(tag);
            await _tagRepository.SaveChangeAsync();

            return new SuccessResult("Tag deleted successfully.");
        }

        public async Task<IDataResult<List<TagDTO>>> GetAllAsync()
        {
            var tags = await _tagRepository.GetAllAsync();
            var tagDTOs = tags.Adapt<List<TagDTO>>();
            return new SuccessDataResult<List<TagDTO>>(tagDTOs, "All tags retrieved successfully.");
        }

        public async Task<IDataResult<TagDTO>> GetByIdAsync(Guid id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
                return new ErrorDataResult<TagDTO>("Tag not found.");

            var tagDTO = tag.Adapt<TagDTO>();
            return new SuccessDataResult<TagDTO>(tagDTO, "Tag retrieved successfully.");
        }

        public async Task<IDataResult<List<TagDTO>>> GetPopularTagsAsync(int count)
        {
            var tags = await _tagRepository.GetAllAsync(
                orderBy: q => q.OrderByDescending(t => t.PostTags.Count),
                take: count);

            var tagDTOs = tags.Select(t => t.Adapt<TagDTO>()).ToList();
            return new SuccessDataResult<List<TagDTO>>(tagDTOs, "Popular tags retrieved successfully.");
        }

        public async Task<IDataResult<List<TagDTO>>> GetTagsByPostIdAsync(Guid postId)
        {
            var tags = await _tagRepository.GetAllAsync(
                filter: t => t.PostTags.Any(pt => pt.PostId == postId));

            var tagDTOs = tags.Select(t => t.Adapt<TagDTO>()).ToList();
            return new SuccessDataResult<List<TagDTO>>(tagDTOs, "Tags retrieved successfully.");
        }

        public async Task<IDataResult<List<TagDTO>>> SearchTagsAsync(string searchTerm)
        {
            var tags = await _tagRepository.GetAllAsync(
                filter: t => t.Name.Contains(searchTerm));

            var tagDTOs = tags.Select(t => t.Adapt<TagDTO>()).ToList();
            return new SuccessDataResult<List<TagDTO>>(tagDTOs, "Tags retrieved successfully.");
        }

        public async Task<IResult> ToggleTagActivationAsync(Guid tagId)
        {
            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
                return new ErrorResult("Tag not found.");

            tag.IsActive = !tag.IsActive;
            tag.UpdatedAt = DateTime.UtcNow;

            await _tagRepository.UpdateAsync(tag);
            await _tagRepository.SaveChangeAsync();

            return new SuccessResult("Tag activation toggled successfully.");
        }

        public async Task<IResult> UpdateAsync(TagUpdateDTO tagUpdateDTO)
        {
            var tag = await _tagRepository.GetByIdAsync(tagUpdateDTO.Id);
            if (tag == null)
                return new ErrorResult("Tag not found.");

            tagUpdateDTO.Adapt(tag);
            tag.UpdatedAt = DateTime.UtcNow;

            await _tagRepository.UpdateAsync(tag);
            await _tagRepository.SaveChangeAsync();

            return new SuccessResult("Tag updated successfully.");
        }
    }
}