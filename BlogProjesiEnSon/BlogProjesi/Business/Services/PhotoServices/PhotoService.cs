using Business.DTOs.PhotoDTOs;
using Domain.Entites;
using Infrastructure.Repositories.PhotoRepository;
using Mapster;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Business.Services.PhotoServices
{
    public class PhotoService : IPhotoService
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly IWebHostEnvironment _env;

        public PhotoService(IPhotoRepository photoRepository, IWebHostEnvironment env)
        {
            _photoRepository = photoRepository;
            _env = env;
        }

        public async Task<List<PhotoListDTO>> GetAllPhotosAsync()
        {
            var photos = await _photoRepository.GetAllAsync();
            return photos.Adapt<List<PhotoListDTO>>();
        }

        public async Task<PhotoListDTO> GetByIdAsync(Guid id)
        {
            try
            {
                var photo = await _photoRepository.GetByIdAsync(id);

                if (photo == null)
                    throw new Exception("Fotoğraf bulunamadı!");

                return photo.Adapt<PhotoListDTO>();
            }
            catch (Exception ex)
            {
                // burayı loglamak istersen logla
                throw new Exception("GetByIdAsync hata: " + ex.Message);
            }
        }


        public async Task<List<PhotoListDTO>> GetLatestPhotosAsync(int count)
        {
            var photos = await _photoRepository.GetLatestPhotosAsync(count);
            return photos.Adapt<List<PhotoListDTO>>();
        }


        public async Task CreateAsync(PhotoCreateDTO dto)
        {
            var photo = dto.Adapt<Photo>();

            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid() + Path.GetExtension(dto.Image.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

            photo.ImageUrl = "/uploads/" + fileName;
            photo.CreatedDate = DateTime.Now;
            await _photoRepository.AddAsync(photo);
            photo.CreatedDate = DateTime.Now;
            await _photoRepository.SaveChangeAsync();
        }

        public async Task UpdateAsync(PhotoUpdateDTO dto)
        {
            var photo = await _photoRepository.GetAsync(x => x.Id == dto.Id);
            if (photo == null) return;

            photo.Title = dto.Title;

            if (dto.Image != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid() + Path.GetExtension(dto.Image.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                photo.ImageUrl = "/uploads/" + fileName;
            }

            await _photoRepository.UpdateAsync(photo);
            await _photoRepository.SaveChangeAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var photo = await _photoRepository.GetAsync(x => x.Id == id);
            if (photo != null)
            {
                await _photoRepository.DeleteAsync(photo);
                await _photoRepository.SaveChangeAsync();
            }
        }
    }
}
