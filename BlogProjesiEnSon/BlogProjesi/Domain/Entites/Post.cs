using Domain.Core.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entites
{
    public class Post:BaseEntity
    {
        public Post()
        {
            Comments = new List<Comment>();
            LikedUsers = new List<AppUserPost>();
        }
        public string Title { get; set; }
        public string Content { get; set; }
        //public string? Url { get; set; }
        public string Excerpt { get; set; }
        public int ViewCount { get; set; } = 0;
        public string? Slug { get; set; }
        public string? FeaturedImage { get; set; }
        public string? VideoUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Postu yazan kullanıcı (Admin olacak)
        public Guid? UserId { get; set; }
        public int LikeCount { get; set; } = 0;
        public virtual AppUser? User { get; set; }
        public virtual Category Category { get; set; }
        public Guid CategoryId { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<PostTag> PostTag { get; set; } = new List<PostTag>();
        public virtual ICollection<AppUserPost> LikedUsers { get; set; } = new List<AppUserPost>();
    }
}
