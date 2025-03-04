using Business.DTOs.CommentsDTOs;
using Business.DTOs.PostTagDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.PostDTOs
{
    public class PostDTO
    {
        public Guid Id { get; set; }  // BaseEntity'den alınan Id
        public string Title { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
        public string? VideoUrl { get; set; }
        public string Excerpt { get; set; }
        public string? Slug { get; set; }
        public string Author { get; set; }
        public string FeaturedImage { get; set; }
        public int LikeCount { get; set; }
        public int ViewCount { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }  // Admin'in adı veya kullanıcı adı
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<CommentDTO> Comments { get; set; } = new List<CommentDTO>(); // Posta ait yorumları içerir
        public ICollection<PostTagDTO> PostTags { get; set; } = new List<PostTagDTO>(); // Post'a ait etiketleri içerir

    }
}
