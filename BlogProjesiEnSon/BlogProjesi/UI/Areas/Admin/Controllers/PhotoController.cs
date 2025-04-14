using Business.DTOs.PhotoDTOs;
using Business.Services.PhotoServices;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace UI.Areas.Admin.Controllers
{
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

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _photoService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
