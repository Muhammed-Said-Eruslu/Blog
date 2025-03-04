using Domain.Entites;
using Infrastructure.DataAccess.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.CategoryRepository
{
    public class CategoryRepository : EFBaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(DbContext context) : base(context)
        {
        }

        public void RemoveRange(IEnumerable<Category> entities)
        {
           
            _context.Set<Category>().RemoveRange(entities);
        }
    }
}