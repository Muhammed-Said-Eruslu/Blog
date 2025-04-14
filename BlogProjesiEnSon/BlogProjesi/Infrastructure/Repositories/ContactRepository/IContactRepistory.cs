using Domain.Entites;
using Infrastructure.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.ContactRepository
{
    public interface IContactRepistory :  IAsyncRepository, IAsyncFindableRepository<Contact>, IAsyncInsertableRepository<Contact>, IAsyncQueryableRepository<Contact>, IAsyncDeletableRepository<Contact>, IAsyncUpdatableRepository<Contact>, IAsyncTransactionRepository 
    {
    }
}
