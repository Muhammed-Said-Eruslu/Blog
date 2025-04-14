using Domain.Entites;
using Infrastructure.DataAccess.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.CommentRepository
{
    public class CommentRepository : EFBaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(DbContext context) : base(context)
        {
        }

        public async Task<int> CountAsync(Expression<Func<Comment, bool>> filter = null)
        {
            IQueryable<Comment> query = _context.Set<Comment>();

            // Eğer filtre varsa, filtreyi uygula
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Veritabanında sayma işlemini asenkron olarak gerçekleştir
            return await query.CountAsync();
        }

        public async Task<IEnumerable<Comment>> GetAllAsync(
            Func<IQueryable<Comment>, IOrderedQueryable<Comment>> orderBy = null,
            Expression<Func<Comment, bool>> filter = null,
            int? skip = null,
            int? take = null)
        {
            IQueryable<Comment> query = _context.Set<Comment>();

            // Filtre varsa uygula
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Sıralama varsa uygula
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Skip varsa uygula
            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            // Take varsa uygula
            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            // Asenkron olarak verileri getir
            return await query.ToListAsync();
        }

        public async Task<List<Comment>> GetAllIncludingAsync(Expression<Func<Comment, bool>> filter, params Expression<Func<Comment, object>>[] includes)
        {
            IQueryable<Comment> query = _context.Set<Comment>().Where(filter);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }

    }
}
