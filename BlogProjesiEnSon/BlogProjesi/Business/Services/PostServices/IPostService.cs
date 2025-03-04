using Business.DTOs.CommentsDTOs;
using Business.DTOs.PostDTOs;
using Domain.Core.İnterfaces;
using Domain.Enums;
using Domain.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services.PostServices
{
    public interface IPostService
    {
        Task<IResult> AddAsync(PostCreateDTO postCreateDTO);
        Task<IDataResult<PostDTO>> GetByIdAsync(Guid id);
        Task<IDataResult<List<PostListDTO>>> GetAllAsync();
        Task<IResult> UpdateAsync(PostUpdateDTO postUpdateDTO);
        Task<IResult> DeleteAsync(Guid id);
        Task<IDataResult<List<PostListDTO>>> GetPostsByCategoryIdAsync(Guid categoryId);
        Task<IDataResult<List<PostListDTO>>> GetPostsByUserIdAsync(Guid userId);
        Task<IDataResult<List<PostListDTO>>> SearchPostsAsync(string searchTerm);
        Task<IResult> UpdatePostStatusAsync(Guid postId, Status status);
        Task<IResult> AddCommentToPostAsync(Guid postId, CommentCreateDTO commentCreateDTO);
        Task<IResult> AddTagsToPostAsync(Guid postId, List<Guid> tagIds);
        Task<IResult> RemoveTagsFromPostAsync(Guid postId, List<Guid> tagIds);
        Task<IDataResult<PostDetailDTO>> GetPostDetailsAsync(Guid postId);
        Task<IDataResult<List<PostListDTO>>> GetPopularPostsAsync(int count);
        Task<IDataResult<PostDTO>> GetPostBySlugAsync(string slug);
        Task<IResult> BulkDeletePostsAsync(List<Guid> postIds);
        Task<IDataResult<PostStatisticsDTO>> GetPostStatisticsAsync();
        Task<IDataResult<PostDetailDTO>> GetPostDetailsWithIncludesAsync(Guid postId);
        Task<IDataResult<T>> GetPostWithIncludesAsync<T>(Guid postId, Func<IQueryable<T>, IQueryable<T>> include) where T : class, IEntity;
        public Task<IResult> UpdateVideoUrlAsync(Guid postId, string videoUrl);
        Task<IDataResult<List<string>>> GetAllSlugsAsync();
        Task<IDataResult<List<PostListDTO>>> GetAllWithCountAsync();

        Task<IResult> IncreaseViewCountAsync(Guid postId);
        Task<IResult> IncreaseLikeCountAsync(Guid postId);
        Task<IDataResult<T>> GetPostWithIncludesAsync<T>(string slug, Func<IQueryable<T>, IQueryable<T>> include) where T : class, IEntity; 




    }
}
