using Business.DTOs.PostTagDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.TagDTOs
{
    public class TagDTO
    {
        public Guid Id { get; set; }  // BaseEntity'den alınan Id
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<PostTagDTO> PostTags { get; set; }
        public Guid? PostId => PostTags?.FirstOrDefault()?.PostId;
    }
}
