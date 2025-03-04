using Business.DTOs.TagDTOs;
using Business.Services.TagServices;
using Domain.Utilities.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Tag")]
    public class TagController : Controller
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        // 1️⃣ Tüm Etiketleri Listele
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var result = await _tagService.GetAllAsync();
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(new List<TagDTO>());
            }

            return View(result.Data);
        }



        // 2️⃣ Yeni Etiket Ekle (Form Sayfası)
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // 3️⃣ Yeni Etiket Ekleme İşlemi
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TagCreateDTO tagCreateDTO)
        {
            if (!ModelState.IsValid)
                return View(tagCreateDTO);

            var result = await _tagService.AddAsync(tagCreateDTO);
            TempData["SuccessMessage"] = result.IsSucces ? "Etiket başarıyla eklendi!" : result.Message;
            return RedirectToAction(nameof(Index));
        }

        // 4️⃣ Etiketi Düzenle (Form Sayfası)
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _tagService.GetByIdAsync(id);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Adapt<TagUpdateDTO>());
        }

        // 5️⃣ Etiketi Güncelleme İşlemi
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TagUpdateDTO tagUpdateDTO)
        {
            if (!ModelState.IsValid)
                return View(tagUpdateDTO);

            var result = await _tagService.UpdateAsync(tagUpdateDTO);
            TempData["SuccessMessage"] = result.IsSucces ? "Etiket başarıyla güncellendi!" : result.Message;
            return RedirectToAction(nameof(Index));
        }

        
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _tagService.GetByIdAsync(id);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // 7️⃣ Etiketi Silme Onayı (Sayfa Gösterimi)
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _tagService.GetByIdAsync(id);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        [HttpPost("DeleteConfirmed/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var result = await _tagService.DeleteAsync(id);

            if (result.IsSucces)
            {
                return Json(new { success = true, message = "Etiket başarıyla silindi!" });
            }

            return Json(new { success = false, message = result.Message });
        }

        // 9️⃣ Bir Gönderiye Ait Tüm Etiketleri Getir
        [HttpGet("GetTagsByPostId/{postId}")]
        public async Task<IActionResult> GetTagsByPostId(Guid postId)
        {
            var result = await _tagService.GetTagsByPostIdAsync(postId);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            return View("GetTagsByPostId", result.Data);
        }

        // Popüler Etiketleri Getir
        [HttpGet("GetPopularTags/{count}")]
        public async Task<IActionResult> GetPopularTags(int count)
        {
            var result = await _tagService.GetPopularTagsAsync(count);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            return View("GetPopularTags", result.Data);
        }

      
        [HttpGet("Search")]
        public async Task<IActionResult> Search(string searchTerm)
        {
            var result = await _tagService.SearchTagsAsync(searchTerm);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            return View("SearchResults", result.Data);
        }


        [HttpPost("ToggleTagActivation/{tagId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTagActivation(Guid tagId)
        {
            var result = await _tagService.ToggleTagActivationAsync(tagId);

            if (result.IsSucces)
            {
                return Json(new { success = true, message = "Etiket durumu değiştirildi!" });
            }

            return Json(new { success = false, message = result.Message });
        }
    }
}
