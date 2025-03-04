using Domain.Entites;
using Infrastructure.DataAccess.Interface;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.TagRepository
{
    public interface ITagRepostiry : IAsyncRepository, IAsyncFindableRepository<Tag>, IAsyncInsertableRepository<Tag>, IAsyncQueryableRepository<Tag>, IAsyncDeletableRepository<Tag>, IAsyncUpdatableRepository<Tag>, IAsyncTransactionRepository
    {
        Task<IEnumerable<Tag>> GetAllAsync(
    Expression<Func<Tag, bool>> filter = null,
    Func<IQueryable<Tag>, IOrderedQueryable<Tag>> orderBy = null,
    int? take = null);
    }
}
