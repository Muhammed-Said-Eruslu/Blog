using Domain.Entites;
using Infrastructure.DataAccess.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.SubscriberRepository
{
    public class SubscriberRepository : EFBaseRepository<Subscriber>, ISubscriberRepository
    {
        public SubscriberRepository(DbContext context) : base(context)
        {
        }
    }
}
