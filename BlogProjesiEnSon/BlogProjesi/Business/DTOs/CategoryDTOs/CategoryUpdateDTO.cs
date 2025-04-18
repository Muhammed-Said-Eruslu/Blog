using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.CategoryDTOs
{
    public class CategoryUpdateDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string? Description { get; set; }
        public int PostCount { get; set; } // Post sayısı
        public DateTime CreatedDate { get; set; } // Kategori oluşturulma tarihi
    }
}
