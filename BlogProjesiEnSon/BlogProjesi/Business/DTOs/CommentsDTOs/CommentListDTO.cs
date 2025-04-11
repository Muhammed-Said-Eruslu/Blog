using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.CommentsDTOs
{
    public class CommentListDTO
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? PostId { get; set; }
        public string? AuthorName { get; set; }
        public Guid? ParentCommentId { get; set; }
        public string AuthorProfileImage { get; set; }

    }
}
