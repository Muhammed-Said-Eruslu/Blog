using Business.DTOs.CommentsDTOs;
using Domain.Entites;
using Domain.Utilities.Concretes;
using Domain.Utilities.Interfaces;
using Infrastructure.Repositories.CommentRepository;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services.CommentsServices
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        public async Task<IResult> AddAsync(CommentCreateDTO commentCreateDTO)
        {
            try
            {
                var newComment = commentCreateDTO.Adapt<Comment>();

                newComment.UserId = commentCreateDTO.UserId;

                await _commentRepository.AddAsync(newComment);
                await _commentRepository.SaveChangeAsync();

                return new SuccessResult("Yorum Ekleme Başarılı.");
            }
            catch (Exception ex)
            {
                return new ErrorResult("Hata: " + ex.Message);
            }
        }


        public async Task<IDataResult<object>> CheckIfUserCommentedAsync(Guid postId, Guid userId)
        {
            var hasCommented = await _commentRepository.AnyAsync(c => c.PostId == postId && c.UserId == userId);
            return new SuccessDataResult<object>(hasCommented, "Check completed successfully.");
        }

        public async Task<IResult> DeleteAsync(Guid id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
                return new ErrorResult("Yorum Bulunamadı.");

            try
            {
                await _commentRepository.DeleteAsync(comment);
                await _commentRepository.SaveChangeAsync();
                return new SuccessResult("Yorum Silme Başarılı.");
            }
            catch (Exception ex)
            {
                return new ErrorResult("Hata: " + ex.Message);
            }
        }

        public async Task<IResult> FlagCommentAsInappropriateAsync(Guid commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
                return new ErrorResult("Yorum Bulunamadı.");

            comment.IsFlagged = true; // IsFlagged özelliği Comment entity'sine eklenmeli
            await _commentRepository.UpdateAsync(comment);
            await _commentRepository.SaveChangeAsync();

            return new SuccessResult("Yorum uygunsuz olarak işaretlendi.");
        }

        public async Task<IDataResult<List<CommentListDTO>>> GetAllAsync()
        {
            var comments = await _commentRepository.GetAllAsync();
            var commentDTOs = comments.Select(c => c.Adapt<CommentListDTO>()).ToList();

            if (!commentDTOs.Any())
                return new ErrorDataResult<List<CommentListDTO>>(commentDTOs, "Yorum bulunamadı.");

            return new SuccessDataResult<List<CommentListDTO>>(commentDTOs, "Yorumlar Başarıyla Getirildi.");
        }

        public async Task<IDataResult<CommentDTO>> GetByIdAsync(Guid id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
                return new ErrorDataResult<CommentDTO>(null, "Yorum Bulunamadı.");

            var commentDTO = comment.Adapt<CommentDTO>();
            return new SuccessDataResult<CommentDTO>(commentDTO, "Yorum Başarıyla Getirildi.");
        }
        public async Task<IDataResult<List<CommentListDTO>>> GetByPostIdAsync(Guid postId)
        {
            var comments = await _commentRepository.GetAllIncludingAsync(
                c => c.PostId == postId && c.ParentCommentId == null,
                c => c.User // Kullanıcı bilgilerini dahil ediyoruz
            );

            var commentDTOs = comments.Select(c => new CommentListDTO
            {
                Id = c.Id,
                PostId = c.PostId,
                AuthorName = c.User?.FullName ?? "Anonim Kullanıcı",
                AuthorProfileImage = !string.IsNullOrEmpty(c.User?.ProfileImage) ? c.User.ProfileImage : "/images/default-avatar.png",
                Content = c.Content,
                CreatedDate = c.CreatedAt,
                ParentCommentId = c.ParentCommentId
            }).ToList();

            return new SuccessDataResult<List<CommentListDTO>>(commentDTOs, "Yorumlar başarıyla getirildi.");
        }




        public async Task<IDataResult<List<CommentListDTO>>> GetByUserIdAsync(Guid userId)
        {
            var comments = await _commentRepository.GetAllAsync(c => c.UserId == userId);
            var commentDTOs = comments.Select(c => c.Adapt<CommentListDTO>()).ToList();

            if (!commentDTOs.Any())
                return new ErrorDataResult<List<CommentListDTO>>(commentDTOs, "Bu kullanıcı için yorum bulunamadı.");

            return new SuccessDataResult<List<CommentListDTO>>(commentDTOs, "Yorum Başarııyla Getirildi.");
        }

        public async Task<IDataResult<object>> GetCommentCountByPostAsync(Guid postId)
        {
            var commentCount = await _commentRepository.CountAsync(c => c.PostId == postId);
            return new SuccessDataResult<object>(commentCount, "Yorum sayısı başarıyla alındı.");
        }

        public async Task<IDataResult<List<CommentListDTO>>> GetPaginatedByPostAsync(Guid postId, int pageNumber, int pageSize)
        {
            var comments = await _commentRepository.GetAllAsync(
                filter: c => c.PostId == postId,
                skip: (pageNumber - 1) * pageSize,
                take: pageSize);

            var commentDTOs = comments.Select(c => c.Adapt<CommentListDTO>()).ToList();

            if (!commentDTOs.Any())
                return new ErrorDataResult<List<CommentListDTO>>(commentDTOs, "Bu yazıya yorum bulunamadı.");

            return new SuccessDataResult<List<CommentListDTO>>(commentDTOs, "Yorum Başarıyla Getirildi.");
        }

        public async Task<IDataResult<List<CommentListDTO>>> GetRecentCommentsAsync(int count)
        {
            var comments = await _commentRepository.GetAllAsync(
                orderBy: q => q.OrderByDescending(c => c.CreatedAt),
                take: count);

            var commentDTOs = comments.Select(c => c.Adapt<CommentListDTO>()).ToList();

            if (!commentDTOs.Any())
                return new ErrorDataResult<List<CommentListDTO>>(commentDTOs, "Yorum yok.");

            return new SuccessDataResult<List<CommentListDTO>>(commentDTOs, "Son yorumlar başarıyla alındı.");
        }

        public async Task<IDataResult<List<CommentListDTO>>> GetRepliesAsync(Guid parentCommentId)
        {
            var replies = await _commentRepository.GetAllAsync(c => c.ParentCommentId == parentCommentId);
            var replyDTOs = replies.Select(r => r.Adapt<CommentListDTO>()).ToList();

            return new SuccessDataResult<List<CommentListDTO>>(replyDTOs, "Yanıtlar başarıyla alındı.");
        }

        public async Task<IDataResult<CommentWithRepliesDTO>> GetWithRepliesAsync(Guid commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
                return new ErrorDataResult<CommentWithRepliesDTO>(null, "Yorum Bulunamadı.");

            var replies = await _commentRepository.GetAllAsync(c => c.ParentCommentId == commentId);
            var commentWithReplies = new CommentWithRepliesDTO
            {
                Id = comment.Id,
                Content = comment.Content,
                AuthorName = comment.AuthorName,
                CreatedAt = comment.CreatedDate,
                Replies = replies.Select(r => r.Adapt<CommentListDTO>()).ToList()
            };

            return new SuccessDataResult<CommentWithRepliesDTO>(commentWithReplies, "Comment with replies retrieved successfully.");
        }

        public async Task<IResult> ToggleCommentVisibilityAsync(Guid commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
                return new ErrorResult("Comment not found.");

            comment.IsVisible = !comment.IsVisible; // IsVisible özelliği Comment entity'sine eklenmeli
            await _commentRepository.UpdateAsync(comment);
            await _commentRepository.SaveChangeAsync();

            return new SuccessResult("Yorum görünürlüğü başarıyla değiştirildi.");
        }

        public async Task<IResult> UpdateAsync(CommentUpdateDTO commentUpdateDTO)
        {
            var existingComment = await _commentRepository.GetByIdAsync(commentUpdateDTO.Id);
            if (existingComment == null)
                return new ErrorResult("Comment not found.");

            existingComment.Content = commentUpdateDTO.Content;

            try
            {
                await _commentRepository.UpdateAsync(existingComment);
                await _commentRepository.SaveChangeAsync();
                return new SuccessResult("Comment updated successfully.");
            }
            catch (Exception ex)
            {
                return new ErrorResult("Error: " + ex.Message);
            }
        }

        public async Task<IResult> UpdateAuthorInfoAsync(Guid commentId, string authorName, string authorEmail)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
                return new ErrorResult("Yorum Bulunamadı.");

            comment.AuthorName = authorName;
            comment.AuthorEmail = authorEmail;

            await _commentRepository.UpdateAsync(comment);
            await _commentRepository.SaveChangeAsync();

            return new SuccessResult("Yazar bilgileri başarıyla güncellendi.");
        }
    }
}
