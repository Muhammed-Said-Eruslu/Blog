using Domain.Entites;
using Infrastructure.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.SubscriberRepository
{
    public interface ISubscriberRepository : IAsyncRepository, IAsyncFindableRepository<Subscriber>, IAsyncInsertableRepository<Subscriber>, IAsyncQueryableRepository<Subscriber>, IAsyncDeletableRepository<Subscriber>, IAsyncUpdatableRepository<Subscriber>, IAsyncTransactionRepository
    {
    }
}
