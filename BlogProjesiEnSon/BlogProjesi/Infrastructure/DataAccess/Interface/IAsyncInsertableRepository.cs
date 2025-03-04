using Domain.Core.BaseEntity;

namespace Infrastructure.DataAccess.Interface
{
    public interface IAsyncInsertableRepository<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity> AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);

    }
}
