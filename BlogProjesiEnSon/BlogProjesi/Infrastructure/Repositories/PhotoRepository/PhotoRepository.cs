using Domain.Entites;
using Infrastructure.DataAccess.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.PhotoRepository
{
    public class PhotoRepository : EFBaseRepository<Photo>, IPhotoRepository
    {
        public PhotoRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<Photo>> GetLatestPhotosAsync(int count)
        {
            return await _context.Set<Photo>()
                .OrderByDescending(p => p.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

    }
}
