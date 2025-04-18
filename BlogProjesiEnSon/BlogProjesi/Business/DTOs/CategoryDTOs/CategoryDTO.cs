using Business.DTOs.PostDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.CategoryDTOs
{
    public class CategoryDTO
    {
        public Guid Id { get; set; }  // BaseEntity'den alınan Id
        public string Name { get; set; }
        public string Slug { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        // Kategorideki toplam post sayısı
        public int PostCount { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public ICollection<Guid> PostIds { get; set; } = new List<Guid>(); // Postların Id'lerini içeren bir koleksiyon
        public List<PostDTO> Posts { get; set; }
    }
}
