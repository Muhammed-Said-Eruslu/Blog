using Domain.Entites;
using Infrastructure.DataAccess.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.ContactRepository
{
    public class ContactRepository : EFBaseRepository<Contact>, IContactRepistory
    {
        public ContactRepository(DbContext context) : base(context)
        {
        }
    }
}
