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
            var topLevelComments = allComments
                .Where(c => c.ParentCommentId == null)
                .Select(c => new CommentDTO
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    AuthorName = !string.IsNullOrEmpty(c.User?.FullName) ? c.User.FullName : c.AuthorName ?? "Anonim",
                    AuthorProfileImage = !string.IsNullOrEmpty(c.User?.ProfileImage) ? c.User.ProfileImage : "/images/default-avatar.png",
                    UserName = !string.IsNullOrEmpty(c.User?.UserName) ? c.User.UserName : "Anonim",
                    Replies = allComments
                        .Where(r => r.ParentCommentId == c.Id)
                        .Select(r => new CommentDTO
                        {
                            Id = r.Id,
                            Content = r.Content,
                            CreatedAt = r.CreatedAt,
                            AuthorName = !string.IsNullOrEmpty(r.User?.FullName) ? r.User.FullName : r.AuthorName ?? "Anonim",
                            AuthorProfileImage = !string.IsNullOrEmpty(r.User?.ProfileImage) ? r.User.ProfileImage : "/images/default-avatar.png",
                            UserName = !string.IsNullOrEmpty(r.User?.UserName) ? r.User.UserName : "Anonim"
                        }).ToList()
                })
                .ToList();

            return topLevelComments;
        }
    }
}
