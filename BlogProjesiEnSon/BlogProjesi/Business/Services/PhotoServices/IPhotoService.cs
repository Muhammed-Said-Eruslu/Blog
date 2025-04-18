using Business.DTOs.PhotoDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services.PhotoServices
{
    public interface IPhotoService
    {
        Task<List<PhotoListDTO>> GetAllPhotosAsync();
        Task<List<PhotoListDTO>> GetLatestPhotosAsync(int count);

        Task<PhotoListDTO> GetByIdAsync(Guid id);
        Task CreateAsync(PhotoCreateDTO photoCreateDTO);
        Task UpdateAsync(PhotoUpdateDTO photoUpdateDTO);
        Task<bool> DeleteAsync(Guid id);
    }
}
