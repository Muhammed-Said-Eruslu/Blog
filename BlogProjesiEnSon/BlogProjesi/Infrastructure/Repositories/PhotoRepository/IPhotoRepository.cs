using Domain.Entites;
using Infrastructure.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.PhotoRepository
{
    public interface IPhotoRepository : IAsyncRepository, IAsyncFindableRepository<Photo>, IAsyncInsertableRepository<Photo>, IAsyncQueryableRepository<Photo>, IAsyncDeletableRepository<Photo>, IAsyncUpdatableRepository<Photo>, IAsyncTransactionRepository
    {
        Task<List<Photo>> GetLatestPhotosAsync(int count);
    }
}
