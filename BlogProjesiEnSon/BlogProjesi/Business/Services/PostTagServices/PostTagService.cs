using Business.DTOs.PostDTOs;
using Business.DTOs.PostTagDTOs;
using Business.DTOs.TagDTOs;
using Domain.Entites;
using Domain.Utilities.Concretes;
using Domain.Utilities.Interfaces;
using Infrastructure.Repositories.PostTagRepository;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
namespace Business.Services.PostTagServices
{
    public class PostTagService : IPostTagService
    {
        private readonly IPostTagRepository _postTagRepository;

        public PostTagService(IPostTagRepository postTagRepository)
        {
            _postTagRepository = postTagRepository;
        }

        // CRUD İşlemleri
        public async Task<IResult> AddAsync(PostTagCreateDTO postTagCreateDTO)
        {
            var postTag = postTagCreateDTO.Adapt<PostTag>();
            postTag.CreatedAt = DateTime.UtcNow;

            await _postTagRepository.AddAsync(postTag);
            await _postTagRepository.SaveChangeAsync();

            return new SuccessResult("PostTag added successfully.");
        }

        public async Task<IDataResult<PostTagDTO>> GetByIdAsync(Guid id)
        {
            var postTag = await _postTagRepository.GetByIdAsync(id);
            if (postTag == null)
                return new ErrorDataResult<PostTagDTO>(null, "PostTag not found.");

            var postTagDTO = postTag.Adapt<PostTagDTO>();
            return new SuccessDataResult<PostTagDTO>(postTagDTO, "PostTag retrieved successfully.");
        }

        public async Task<IDataResult<List<PostTagDTO>>> GetAllAsync()
        {
            // Post ve Tag bilgilerini dahil etmek için
            var postTags = await _postTagRepository.GetAllAsync(
                filter: null, // gerekirse predicate
                include: q => q.Include(pt => pt.Post)
                               .Include(pt => pt.Tag)
            );

            // PostTagDTO'ya map'leme (Elle veya Mapster/AutoMapper ile)
            var postTagDTOs = postTags.Select(pt => new PostTagDTO
            {
                Id = pt.Id,
                PostId = pt.PostId,
                TagId = pt.TagId,
                IsActive = pt.IsActive,
                CreatedAt = pt.CreatedAt,
                UpdatedAt = pt.UpdatedAt,
                // Post ve Tag'ın isim alanlarını DTO'ya dolduruyoruz
                PostTitle = pt.Post?.Title,
                TagName = pt.Tag?.Name
            }).ToList();

            if (!postTagDTOs.Any())
                return new ErrorDataResult<List<PostTagDTO>>(postTagDTOs, "No PostTags found.");

            return new SuccessDataResult<List<PostTagDTO>>(postTagDTOs, "PostTags retrieved successfully.");
        }


        public async Task<IResult> UpdateAsync(PostTagUpdateDTO postTagUpdateDTO)
        {
            var postTag = await _postTagRepository.GetByIdAsync(postTagUpdateDTO.Id);
            if (postTag == null)
                return new ErrorResult("PostTag not found.");

            postTag.IsActive = postTagUpdateDTO.IsActive;
            postTag.UpdatedAt = DateTime.UtcNow;

            await _postTagRepository.UpdateAsync(postTag);
            await _postTagRepository.SaveChangeAsync();

            return new SuccessResult("PostTag updated successfully.");
        }

        public async Task<IResult> DeleteAsync(Guid id)
        {
            var postTag = await _postTagRepository.GetByIdAsync(id);
            if (postTag == null)
                return new ErrorResult("PostTag not found.");

            await _postTagRepository.DeleteAsync(postTag);
            await _postTagRepository.SaveChangeAsync();

            return new SuccessResult("PostTag deleted successfully.");
        }

        // Post ve Tag ile İlgili İşlemler
        public async Task<IDataResult<List<TagDTO>>> GetTagsByPostIdAsync(Guid postId)
        {
            var postTags = await _postTagRepository.GetAllAsync(
                filter: pt => pt.PostId == postId,
                include: query => query.Include(pt => pt.Tag)); // Tag'i yükle

            var tagDTOs = postTags.Select(pt => pt.Tag.Adapt<TagDTO>()).ToList();
            return new SuccessDataResult<List<TagDTO>>(tagDTOs, "Tags retrieved successfully.");
        }

        public async Task<IDataResult<List<PostDTO>>> GetPostsByTagIdAsync(Guid tagId)
        {
            var postTags = await _postTagRepository.GetAllAsync(
                filter: pt => pt.TagId == tagId,
                include: query => query.Include(pt => pt.Post)); // Post'u yükle

            var postDTOs = postTags.Select(pt => pt.Post.Adapt<PostDTO>()).ToList();
            return new SuccessDataResult<List<PostDTO>>(postDTOs, "Posts retrieved successfully.");
        }
        public async Task<IResult> AddTagsToPostAsync(Guid postId, List<Guid> tagIds)
        {
            foreach (var tagId in tagIds)
            {
                var postTag = new PostTag
                {
                    PostId = postId,
                    TagId = tagId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _postTagRepository.AddAsync(postTag);
            }

            await _postTagRepository.SaveChangeAsync();
            return new SuccessResult("Tags added to post successfully.");
        }

        public async Task<IResult> RemoveTagsFromPostAsync(Guid postId, List<Guid> tagIds)
        {
            var postTags = await _postTagRepository.GetAllAsync(pt => pt.PostId == postId && tagIds.Contains(pt.TagId));
            if (!postTags.Any())
                return new ErrorResult("No matching PostTags found.");

            _postTagRepository.RemoveRange(postTags);
            await _postTagRepository.SaveChangeAsync();

            return new SuccessResult("Tags removed from post successfully.");
        }


        public async Task<IResult> TogglePostTagActivationAsync(Guid postTagId)
        {
            var postTag = await _postTagRepository.GetByIdAsync(postTagId);
            if (postTag == null)
                return new ErrorResult("PostTag not found.");

            postTag.IsActive = !postTag.IsActive;
            postTag.UpdatedAt = DateTime.UtcNow;

            await _postTagRepository.UpdateAsync(postTag);
            await _postTagRepository.SaveChangeAsync();

            return new SuccessResult("PostTag activation toggled successfully.");
        }

        async Task<IDataResult<object>> IPostTagService.CheckIfTagExistsInPostAsync(Guid postId, Guid tagId)
        {
            var exists = await _postTagRepository.AnyAsync(pt => pt.PostId == postId && pt.TagId == tagId);
            return new SuccessDataResult<object>(exists, "Check completed successfully.");
        }
        public async Task<IResult> RemoveAllTagsFromPostAsync(Guid postId)
        {
            var postTags = await _postTagRepository.GetAllAsync(pt => pt.PostId == postId);
            if (!postTags.Any())
                return new ErrorResult("No tags found for the post.");

            _postTagRepository.RemoveRange(postTags);
            await _postTagRepository.SaveChangeAsync();

            return new SuccessResult("All tags removed from the post successfully.");
        }

    }
}