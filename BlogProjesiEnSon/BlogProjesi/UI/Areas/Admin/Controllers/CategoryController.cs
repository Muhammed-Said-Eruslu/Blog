using Business.DTOs.CategoryDTOs;
using Business.Services.CategoryServices;
using Domain.Utilities.Concretes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // Tüm kategorileri listeleme
        public async Task<IActionResult> Index()
        {
            var result = await _categoryService.GetAllAsync();
            return View(result);
        }

        // Kategori detayları
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // Slug'a göre kategori görüntüleme
        public async Task<IActionResult> DetailsBySlug(string slug)
        {
            var result = await _categoryService.GetBySlugAsync(slug);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            return View("Details", result.Data);
        }

        // Yeni kategori oluşturma formu
        public IActionResult Create()
        {
            return View();
        }

        // Kategori oluşturma işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateDTO model)
        {
            if (ModelState.IsValid)
            {
                var result = await _categoryService.AddAsync(model);
                if (result.IsSucces)
                {
                    TempData["SuccessMessage"] = "Kategori başarıyla oluşturuldu!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", result.Message);
            }
            return View(model);
        }

        // Kategori düzenleme formu
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var updateDto = new CategoryUpdateDTO
            {
                Id = result.Data.Id,
                Name = result.Data.Name,
                Description = result.Data.Description
            };

            return View(updateDto);
        }

        // Kategori güncelleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryUpdateDTO model)
        {
            if (ModelState.IsValid)
            {
                var result = await _categoryService.UpdateAsync(model);
                if (result.IsSucces)
                {
                    TempData["SuccessMessage"] = "Kategori başarıyla güncellendi!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", result.Message);
            }
            return View(model);
        }

        // Kategori silme onay sayfası
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var result = await _categoryService.DeleteAsync(id);  // Kategori silme işlemi

            if (result.IsSucces)
            {
                return Json(new { success = true });  // Başarıyla silindiğinde JSON döneriz
            }
            else
            {
                return Json(new { success = false, message = result.Message });  // Hata olursa JSON döneriz
            }
        }


        // Toplu silme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["ErrorMessage"] = "Lütfen silinecek kategorileri seçiniz!";
                return RedirectToAction(nameof(Index));
            }

            var result = await _categoryService.BulkDeleteAsync(ids);
            if (result.IsSucces)
            {
                TempData["SuccessMessage"] = "Seçili kategoriler başarıyla silindi!";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}