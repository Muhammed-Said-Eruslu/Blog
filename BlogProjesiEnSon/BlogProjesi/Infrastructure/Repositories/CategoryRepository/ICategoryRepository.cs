using Domain.Entites;
using Infrastructure.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.CategoryRepository
{
    public interface ICategoryRepository : IAsyncRepository, IAsyncFindableRepository<Category>, IAsyncInsertableRepository<Category>, IAsyncQueryableRepository<Category>, IAsyncDeletableRepository<Category>, IAsyncUpdatableRepository<Category>, IAsyncTransactionRepository
    {
        void RemoveRange(IEnumerable<Category> entities);
    }
}
