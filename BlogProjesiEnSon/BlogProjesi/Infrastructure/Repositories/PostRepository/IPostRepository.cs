using Domain.Core.İnterfaces;
using Domain.Entites;
using Infrastructure.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.PostRepository
{
    public interface IPostRepository : IAsyncRepository, IAsyncFindableRepository<Post>, IAsyncInsertableRepository<Post>, IAsyncQueryableRepository<Post>, IAsyncDeletableRepository<Post>, IAsyncUpdatableRepository<Post>, IAsyncTransactionRepository
    {
        void RemoveRange(IEnumerable<Post> entities);
        Task<int> CountAsync();
        Task<IEnumerable<Post>> GetAllAsync(Func<IQueryable<Post>, IOrderedQueryable<Post>> orderBy = null, int? take = null);
        Task<IEnumerable<Post>> GetAllAsync(Expression<Func<Post, bool>> predicate, Func<IQueryable<Post>, IQueryable<Post>> include);
        Task<T> GetAsyncWithIncludes<T>(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>> include) where T : class, IEntity;
        Task<Post> GetAsync(Expression<Func<Post, bool>> predicate, Func<IQueryable<Post>, IQueryable<Post>> include);
        Task<List<Post>> GetAllAsync(
            Expression<Func<Post, bool>> predicate = null,
            Func<IQueryable<Post>, IQueryable<Post>> include = null,
            Func<IQueryable<Post>, IOrderedQueryable<Post>> orderBy = null,
            int? take = null
        );

        Task<List<T>> GetAllWithIncludesAsync<T>(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>> include) where T : class, IEntity;
    }
}
