using System;
using System.Collections.Generic;

namespace Business.DTOs.CommentsDTOs
{
    public class CommentWithRepliesDTO
    {
        public CommentWithRepliesDTO()
        {
            Replies = new List<CommentListDTO>();
        }

        public Guid Id { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<CommentListDTO> Replies { get; set; }
    }
}
