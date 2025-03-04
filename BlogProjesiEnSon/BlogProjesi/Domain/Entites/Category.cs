using Domain.Core.BaseEntity;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entites
{
    public class Category:BaseEntity
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string? Description { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
    }
}
