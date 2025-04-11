using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.CategoryDTOs
{
    public class CategoryPostCountDTO
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int PostCount { get; set; }
    }
}
