using Domain.Entites;
using Infrastructure.DataAccess.Interface;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.PostTagRepository
{
    public interface IPostTagRepository : IAsyncRepository, IAsyncFindableRepository<PostTag>, IAsyncInsertableRepository<PostTag>, IAsyncQueryableRepository<PostTag>, IAsyncDeletableRepository<PostTag>, IAsyncUpdatableRepository<PostTag>, IAsyncTransactionRepository
    {
        Task<IEnumerable<PostTag>> GetAllAsync(
         Expression<Func<PostTag, bool>> filter = null,
         Func<IQueryable<PostTag>, IIncludableQueryable<PostTag, object>> include = null);
        void RemoveRange(IEnumerable<PostTag> entities);
    }
}
