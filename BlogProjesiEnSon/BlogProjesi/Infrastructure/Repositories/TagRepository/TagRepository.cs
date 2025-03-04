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

namespace Infrastructure.Repositories.TagRepository
{
    public class TagRepository : EFBaseRepository<Tag>, ITagRepostiry
    {
        public TagRepository(DbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Tag>> GetAllAsync(
        Expression<Func<Tag, bool>> filter = null,
        Func<IQueryable<Tag>, IOrderedQueryable<Tag>> orderBy = null,
        int? take = null)
        {
            var query = _context.Set<Tag>().AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync();
        }
    }
    }

