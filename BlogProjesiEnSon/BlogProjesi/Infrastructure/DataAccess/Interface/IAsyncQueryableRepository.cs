using System.Linq.Expressions;
using Domain.Core.BaseEntity;

namespace Infrastructure.DataAccess.Interface
{
    public interface IAsyncQueryableRepository<TEntity> where TEntity : BaseEntity
    {
        Task<IEnumerable<TEntity>> GetAllAsync(bool tracking = true);
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> expression, bool tracking = true);
    }
}
