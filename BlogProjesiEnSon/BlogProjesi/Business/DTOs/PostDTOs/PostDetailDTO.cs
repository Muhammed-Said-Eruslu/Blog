using Business.DTOs.CommentsDTOs;
using Business.DTOs.PostTagDTOs;
using Business.DTOs.UsersDTOs;

using System;
using System.Collections.Generic;
using Business.DTOs.CategoryDTOs;

namespace Business.DTOs.PostDTOs
{
    public class PostDetailDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
        public string Excerpt { get; set; }
        public string FeaturedImage { get; set; }
        public string VideoUrl { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        // SEO uyumlu URL için slug
        public string Slug { get; set; }

        // Kategori bilgisi
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }

        // Yazar bilgisi
        public Guid AuthorId { get; set; } // ✅ Eksik olan AuthorId eklendi
        public UserDTO Author { get; set; }

        // Yorumlar
        public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();

        // Tag'ler
        public List<PostTagDTO> Tags { get; set; } = new List<PostTagDTO>();

        public List<PostDetailDTO> RelatedTagPosts { get; set; } = new List<PostDetailDTO>();
        public List<PostDetailDTO> RelatedCategoryPosts { get; set; } = new List<PostDetailDTO>();
        public List<PostDetailDTO> PopularPosts { get; set; } = new List<PostDetailDTO>();

        public bool IsLiked { get; set; }

        public Guid UserId { get; set; } // Kullanıcı ID'si
        public List<Guid> TagIds { get; set; } = new List<Guid>(); // Etiket ID'leri
        public List<string> TagNames { get; set; } = new List<string>();

    }
}
