using Business.DTOs.PhotoDTOs;
using Business.Services.PhotoServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace UI.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class PhotoController : Controller
    {
        private readonly IPhotoService _photoService;

        public PhotoController(IPhotoService photoService)
        {
            _photoService = photoService;
        }

        public async Task<IActionResult> Index()
        {
            var photos = await _photoService.GetAllPhotosAsync();
            return View(photos);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var photo = await _photoService.GetByIdAsync(id);
            if (photo == null) return NotFound();
            return View(photo);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(PhotoCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _photoService.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var photo = await _photoService.GetByIdAsync(id);
            if (photo == null) return NotFound();

            var updateDto = new PhotoUpdateDTO
            {
                Id = photo.Id,
                Title = photo.Title,
                ImageUrl = photo.ImageUrl
            };

            return View(updateDto);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(PhotoUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _photoService.UpdateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _photoService.DeleteAsync(id);

            if (result)
            {
                // Başarıyla silindiğinde, JSON formatında bir yanıt döndürüyoruz.
                return Json(new { success = true, message = "Fotoğraf başarıyla silindi." });
            }
            else
            {
                // Hata durumunda, JSON formatında bir hata mesajı döndürüyoruz.
                return Json(new { success = false, message = "Fotoğraf silinirken bir hata oluştu." });
            }
        }

    }
}
