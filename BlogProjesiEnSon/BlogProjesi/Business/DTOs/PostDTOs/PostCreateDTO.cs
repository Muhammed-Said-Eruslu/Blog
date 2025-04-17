using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.PostDTOs
{
    public class PostCreateDTO
    {
        public string Title { get; set; }
        public string Content { get; set; }
        //public string Url { get; set; }
        public string Excerpt { get; set; }
        public string Slug { get; set; }
        public string? FeaturedImage { get; set; }
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
        public string? VideoUrl { get; set; }
        public ICollection<Guid> TagIds { get; set; } = new List<Guid>();  // Posta ait etiketler

        public IFormFile? FeaturedImageFile { get; set; }
        public IFormFile? VideoFile { get; set; }
        public byte[]? RowVersion { get; set; }
    }
}
