using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.PostDTOs
{
    public class PostListDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? VideoUrl { get; set; }
        public string FullName { get; set; }
        public string Excerpt { get; set; }
        public string Slug { get; set; }
        public string FeaturedImage { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid UserId { get; set; }
        public int ViewCount { get; set; }
        public int CommentCount { get; set; }
        public int LikeCount { get; set; }
        public bool IsLiked { get; set; }
    }
}
