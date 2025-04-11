using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.PhotoDTOs
{
    public class PhotoListDTO
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
