using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.PostTagDTOs
{
    public class PostTagCreateDTO
    {
        public Guid PostId { get; set; }
        public Guid TagId { get; set; }
        public bool IsActive { get; set; }
    }
}
