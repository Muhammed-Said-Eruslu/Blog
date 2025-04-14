using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.PhotoDTOs
{
    public class PhotoCreateDTO
    {
        public string Title { get; set; }
        public IFormFile Image { get; set; }
    }
}
