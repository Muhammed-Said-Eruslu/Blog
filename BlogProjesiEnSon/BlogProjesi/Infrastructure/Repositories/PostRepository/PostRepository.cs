using Domain.Core.İnterfaces;
using Domain.Entites;
using Domain.Utilities.Concretes;
using Domain.Utilities.Interfaces;
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

        public async Task<AppUser> GetUserWithLikedPostsAsync(Guid userId)
        {
            return await _context.Set<AppUser>()
                .Include(u => u.LikedPosts) // AppUserPost ilişkisini çek
                .ThenInclude(lp => lp.Post) // AppUserPost içindeki Post bilgisini de çek
                .FirstOrDefaultAsync(u => u.Id == userId);
        }



        public async Task<IDataResult<Post>> ToggleLikeAsync(Guid userId, Guid postId)
        {
            var user = await _context.Set<AppUser>().FindAsync(userId);
            var post = await _context.Set<Post>().FindAsync(postId);

            if (user == null || post == null)
                return new ErrorDataResult<Post>("Kullanıcı veya gönderi bulunamadı.");

            var existingLike = await _context.Set<AppUserPost>()
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PostId == postId);

            if (existingLike != null)
            {
                // Kullanıcı beğenmişse, beğenisini kaldır
                _context.Set<AppUserPost>().Remove(existingLike);
                post.LikeCount--;
            }
            else
            {
                // Kullanıcı beğenmemişse, beğeni ekle
                var newLike = new AppUserPost { UserId = userId, PostId = postId };
                await _context.Set<AppUserPost>().AddAsync(newLike);
                post.LikeCount++;
            }

            // Post güncellemesini de ekledik
            _context.Set<Post>().Update(post);

            await _context.SaveChangesAsync();

            return new SuccessDataResult<Post>(post, "Beğeni durumu güncellendi.");
        }


        public async Task<AppUserPost> GetAppUserPostAsync(Guid userId, Guid postId)
        {
            return await _context.Set<AppUserPost>()
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PostId == postId);
        }

        public async Task AddLikeAsync(AppUserPost like)
        {
            await _context.Set<AppUserPost>().AddAsync(like);
        }

        public async Task RemoveLikeAsync(AppUserPost like)
        {
            _context.Set<AppUserPost>().Remove(like);
        }




    }
}
