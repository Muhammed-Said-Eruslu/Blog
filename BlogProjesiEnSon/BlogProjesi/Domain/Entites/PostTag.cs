using Domain.Core.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entites
{
    public class PostTag:BaseEntity
    {
        public Guid PostId { get; set; }
        public virtual Post Post { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
        public Guid TagId { get; set; }
        public virtual Tag Tag { get; set; }

    }
}
