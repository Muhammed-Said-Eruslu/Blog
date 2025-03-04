using Business.DTOs.TagDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.PostTagDTOs
{
    public class PostTagDTO
    {
        public PostTagDTO()
        {
            AvailableTags = new List<TagDTO>();
            AssignedTags = new List<TagDTO>();
        }
        public Guid Id { get; set; }  // BaseEntity'den alınan Id
        public Guid PostId { get; set; }
        public Guid TagId { get; set; }
        public string Author { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string PostTitle { get; set; }  // Post ile ilişkili başlık
        
        public string TagName { get; set; }  // Etiket adı

        public List<TagDTO> AvailableTags { get; set; } // kullanılabılır etiket listesi
        public List<TagDTO> AssignedTags { get; set; }  // Post'a atanmış etiketler

    }
}
