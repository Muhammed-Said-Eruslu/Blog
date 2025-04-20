using Business.DTOs.CommentsDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helpers
{
    public static class CommentMappingHelper
    {
        public static List<CommentDTO> MapCommentsWithReplies(List<Comment> allComments)
        {
            // Önce tüm yorumları al, sonra ana yorumları filtrele
            var comments = allComments.ToList(); // Materialize ediyoruz
            return comments
                .Where(c => c.ParentCommentId == null)
                .Select(c => MapComment(c, comments))
                .ToList();
        }

        private static CommentDTO MapComment(Comment comment, List<Comment> allComments)
        {
            return new CommentDTO
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                AuthorName = comment.User?.FullName ?? comment.AuthorName ?? "Anonim",
                ProfileImage = comment.User?.ProfileImage ?? "/images/default-avatar.png",
                Replies = allComments
                    .Where(r => r.ParentCommentId == comment.Id)
                    .OrderBy(r => r.CreatedAt) // Alt yorumları tarihe göre sırala
                    .Select(r => MapComment(r, allComments))
                    .ToList()
            };
        }
    }
}
