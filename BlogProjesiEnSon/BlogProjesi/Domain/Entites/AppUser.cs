using Domain.Core.BaseEntity;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entites
{
    public class AppUser:IdentityUser<Guid>
    {
        public string? FullName { get; set; }
        public string Email { get; set; }
        public string? ProfileImage { get; set; }
        public Roles Role { get; set; } = Roles.User;
        public virtual ICollection<Post>? Posts { get; set; } = new List<Post>();
    }
}
