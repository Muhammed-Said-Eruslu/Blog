using Domain.Entites;
using Infrastructure.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.CommentRepository
{
    public interface ICommentRepository : IAsyncRepository, IAsyncFindableRepository<Comment>, IAsyncInsertableRepository<Comment>, IAsyncQueryableRepository<Comment>, IAsyncDeletableRepository<Comment>, IAsyncUpdatableRepository<Comment>, IAsyncTransactionRepository
    {
        Task<IEnumerable<Comment>> GetAllAsync(
       Func<IQueryable<Comment>, IOrderedQueryable<Comment>> orderBy = null,
       Expression<Func<Comment, bool>> filter = null,
       int? skip = null,
       int? take = null);
        Task<int> CountAsync(Expression<Func<Comment, bool>> filter = null);
    }
}
