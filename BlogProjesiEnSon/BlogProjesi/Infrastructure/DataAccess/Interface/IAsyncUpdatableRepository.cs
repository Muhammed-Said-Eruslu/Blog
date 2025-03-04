using Domain.Core.BaseEntity;

namespace Infrastructure.DataAccess.Interface
{
    public interface IAsyncUpdatableRepository<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity> UpdateAsync(TEntity entity);
    }
}
