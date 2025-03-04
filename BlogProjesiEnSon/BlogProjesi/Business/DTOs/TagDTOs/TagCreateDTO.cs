using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.TagDTOs
{
    public class TagCreateDTO
    {
        public string Name { get; set; }
        public string Slug { get; set; } //
        public string Description { get; set; }
    }
}
