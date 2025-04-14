using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.CommentsDTOs
{
    public class CommentCreateDTO
    {
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public string? AuthorEmail { get; set; }
        public Guid PostId { get; set; }
        public Guid? UserId { get; set; }
        public string? PostSlug { get; set; }
        public string? AuthorProfileImage { get; set; }
        public Guid? ParentCommentId { get; set; }
    }
}
