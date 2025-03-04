using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.CommentsDTOs
{
    public class CommentUpdateDTO
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public Guid PostId { get; set; }
        public Guid? UserId { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorEmail { get; set; }
        public Guid? ParentCommentId { get; set; }
    }
}
