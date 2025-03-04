using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.CommentsDTOs
{
    public class CommentDTO
    {
        public Guid Id { get; set; }  // BaseEntity'den alınan Id
        public string Content { get; set; }
        public Guid PostId { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorEmail { get; set; }
        public Guid? ParentCommentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public ICollection<CommentDTO> Replies { get; set; } = new List<CommentDTO>(); // Yorumun yanıtlarını içeren koleksiyon
    }
}
