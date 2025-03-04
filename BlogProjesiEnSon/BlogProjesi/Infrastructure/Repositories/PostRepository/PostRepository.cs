using Domain.Core.İnterfaces;
using Domain.Entites;
using Infrastructure.DataAccess.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.PostRepository
{
    public class PostRepository : EFBaseRepository<Post>, IPostRepository
    {
        private readonly DbContext _context;

        public PostRepository(DbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> CountAsync()
        {
            return await _context.Set<Post>().CountAsync();
        }


        public void RemoveRange(IEnumerable<Post> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentException("Silinecek gönderi bulunamadı.", nameof(entities));

            _context.Set<Post>().RemoveRange(entities);
        }

        public async Task<IEnumerable<Post>> GetAllAsync(Func<IQueryable<Post>, IOrderedQueryable<Post>> orderBy, int? take)
        {
            IQueryable<Post> query = _context.Set<Post>();

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync(); // `List<Post>` zaten `IEnumerable<Post>` implemente ettiği için dönüşüm otomatik yapılır.
        }
        public async Task<T> GetAsyncWithIncludes<T>(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>> include) where T : class,IEntity
        {
            IQueryable<T> query = _context.Set<T>().Where(predicate);
            query = include(query);  // İlişkili verilerle birlikte sorguyu oluşturuyoruz
            return await query.FirstOrDefaultAsync();  // İlk eşleşeni döndürüyoruz
        }

        public async Task<IEnumerable<Post>> GetAllAsync(Expression<Func<Post, bool>> predicate, Func<IQueryable<Post>, IQueryable<Post>> include)
        {
            IQueryable<Post> query = _context.Set<Post>().Where(predicate);
            query = include(query);  // İlişkili verilerle birlikte sorguyu oluşturuyoruz
            return await query.ToListAsync();
        }

        public async Task<Post> GetAsync(Expression<Func<Post, bool>> predicate, Func<IQueryable<Post>, IQueryable<Post>> include)
        {
            IQueryable<Post> query = _context.Set<Post>().Where(predicate);
            query = include(query);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Post>> GetAllAsync(
               Expression<Func<Post, bool>> predicate = null, Func<IQueryable<Post>, IQueryable<Post>> include = null,   Func<IQueryable<Post>, IOrderedQueryable<Post>> orderBy = null, int? take = null)
        {
            IQueryable<Post> query = _context.Set<Post>();

            if (predicate != null)
                query = query.Where(predicate);

            if (include != null)
                query = include(query);

            if (orderBy != null)
                query = orderBy(query);

            if (take.HasValue)
                query = query.Take(take.Value);

            return await query.ToListAsync();
        }

        public async Task<List<T>> GetAllWithIncludesAsync<T>(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>> include) where T : class, IEntity
        {
            IQueryable<T> query = _context.Set<T>().Where(predicate);
            query = include(query);  // İlişkili verilerle birlikte sorguyu oluşturuyoruz
            return await query.ToListAsync();
        }
    }
}
