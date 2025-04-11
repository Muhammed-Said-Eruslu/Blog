using Domain.Core.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entites
{
    public class Subscriber : BaseEntity
    {
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
