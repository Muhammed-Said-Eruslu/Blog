using Domain.Entites;
using Infrastructure.DataAccess.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.PostTagRepository
{
    public class PostTagRepository : EFBaseRepository<PostTag>, IPostTagRepository
    {
        public PostTagRepository(DbContext context) : base(context)
        {
        }

   

        public async Task<IEnumerable<PostTag>> GetAllAsync(Expression<Func<PostTag, bool>> filter = null, Func<IQueryable<PostTag>, IIncludableQueryable<PostTag, object>> include = null)
        {
            var query = _context.Set<PostTag>().AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }

        public void RemoveRange(IEnumerable<PostTag> entities)
        {
            _context.Set<PostTag>().RemoveRange(entities);
        }
    }
}
