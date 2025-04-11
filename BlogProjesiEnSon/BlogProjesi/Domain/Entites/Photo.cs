using Domain.Core.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entites
{
    public class Photo : BaseEntity
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
    }
}
