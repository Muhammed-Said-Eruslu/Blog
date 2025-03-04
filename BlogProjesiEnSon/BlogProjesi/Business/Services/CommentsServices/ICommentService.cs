using Business.DTOs.CommentsDTOs;
using Domain.Utilities.Interfaces;

public interface ICommentService
{
    Task<IResult> AddAsync(CommentCreateDTO commentCreateDTO);
    Task<IResult> DeleteAsync(Guid id);
    Task<IResult> UpdateAsync(CommentUpdateDTO commentUpdateDTO);
    Task<IDataResult<CommentDTO>> GetByIdAsync(Guid id);
    Task<IDataResult<List<CommentListDTO>>> GetByPostIdAsync(Guid postId);
    Task<IDataResult<List<CommentListDTO>>> GetRepliesAsync(Guid parentCommentId);
    Task<IDataResult<List<CommentListDTO>>> GetAllAsync();
    Task<IDataResult<List<CommentListDTO>>> GetByUserIdAsync(Guid userId);
    Task<IResult> ToggleCommentVisibilityAsync(Guid commentId);
    Task<IDataResult<List<CommentListDTO>>> GetPaginatedByPostAsync(Guid postId, int pageNumber, int pageSize);
    Task<IDataResult<object>> GetCommentCountByPostAsync(Guid postId);
    Task<IDataResult<object>> CheckIfUserCommentedAsync(Guid postId, Guid userId);
    Task<IDataResult<List<CommentListDTO>>> GetRecentCommentsAsync(int count);
    Task<IResult> FlagCommentAsInappropriateAsync(Guid commentId);
    Task<IDataResult<CommentWithRepliesDTO>> GetWithRepliesAsync(Guid commentId);
    Task<IResult> UpdateAuthorInfoAsync(Guid commentId, string authorName, string authorEmail);
}