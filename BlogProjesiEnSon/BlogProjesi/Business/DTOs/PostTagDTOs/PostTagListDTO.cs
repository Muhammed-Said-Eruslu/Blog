using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.PostTagDTOs
{
    public class PostTagListDTO
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public string PostTitle { get; set; }
        public Guid TagId { get; set; }
        public string TagName { get; set; }
    }
}
