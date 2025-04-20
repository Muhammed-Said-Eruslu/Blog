using Business.DTOs.PostDTOs;
using Business.DTOs.PostTagDTOs;
using Business.DTOs.TagDTOs;
using Business.Services.PostServices;
using Business.Services.PostTagServices;
using Business.Services.TagServices;
using Domain.Utilities.Concretes;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UI.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]

    public class PostTagController : Controller
    {
        private readonly IPostTagService _postTagService;
        private readonly IPostService _postService;
        private readonly ITagService _tagService;

        public PostTagController(IPostTagService postTagService, IPostService postService, ITagService tagService)
        {
            _postTagService = postTagService;
            _postService = postService;
            _tagService = tagService;
        }

        #region 1. CRUD İşlemleri (Add, GetAll, GetById, Update, Delete)

        // GET: /Admin/PostTag/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // IPostTagService.GetAllAsync()
            var result = await _postTagService.GetAllAsync();
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(new List<PostTagDTO>()); // boş liste gönderiyoruz
            }
            return View(result.Data);
        }

        // GET: /Admin/PostTag/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            // IPostTagService.GetByIdAsync()
            var result = await _postTagService.GetByIdAsync(id);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // GET: /Admin/PostTag/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Post ve Tag listelerini ViewBag'e ekleyelim
            ViewBag.Posts = new SelectList((await _postService.GetAllAsync()).Data, "Id", "Title");
            ViewBag.Tags = new SelectList((await _tagService.GetAllAsync()).Data, "Id", "Name");

            return View();
        }

        // POST: /Admin/PostTag/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostTagCreateDTO model)
        {
            if (!ModelState.IsValid)
            {
                // Eğer validasyon hatası varsa, dropdownlar boş kalmaması için tekrar doldurulmalı
                ViewBag.Posts = new SelectList((await _postService.GetAllAsync()).Data, "Id", "Title");
                ViewBag.Tags = new SelectList((await _tagService.GetAllAsync()).Data, "Id", "Name");

                return View(model);
            }

            var result = await _postTagService.AddAsync(model);
            if (result.IsSucces)
            {
                TempData["SuccessMessage"] = "PostTag başarıyla oluşturuldu!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = result.Message;

            // Hata durumunda tekrar dropdownları doldur
            ViewBag.Posts = new SelectList((await _postService.GetAllAsync()).Data, "Id", "Title");
            ViewBag.Tags = new SelectList((await _tagService.GetAllAsync()).Data, "Id", "Name");

            return View(model);
        }


        // GET: /Admin/PostTag/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _postTagService.GetByIdAsync(id);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            // Gelen PostTagDTO'yu PostTagUpdateDTO'ya dönüştürüyoruz
            var updateDto = new PostTagDTO
            {
                Id = result.Data.Id,
                IsActive = result.Data.IsActive,
                PostTitle = result.Data.PostTitle, // Post Adı Model'e ekleniyor!
                TagName = result.Data.TagName      // Tag Adı Model'e ekleniyor!
            };

            return View(updateDto);
        }


        // POST: /Admin/PostTag/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostTagUpdateDTO model)
        {
            // IPostTagService.UpdateAsync()
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _postTagService.UpdateAsync(model);
            if (result.IsSucces)
            {
                TempData["SuccessMessage"] = "PostTag başarıyla güncellendi!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = result.Message;
            return View(model);
        }

        // GET: /Admin/PostTag/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _postTagService.GetByIdAsync(id);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // POST: /Admin/PostTag/DeleteConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // IPostTagService.DeleteAsync()
            var result = await _postTagService.DeleteAsync(id);
            if (result.IsSucces)
            {
                TempData["SuccessMessage"] = "PostTag başarıyla silindi!";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion



        #region 2. Post - Tag İlişkili Ek Fonksiyonlar

        // 2.1. Bir Post'a Ait Tüm Tag'leri Getir
        [HttpGet]
        public async Task<IActionResult> GetTagsForPost(Guid postId)
        {
            var result = await _postTagService.GetTagsByPostIdAsync(postId);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            return View("TagsByPost", result.Data);
        }

        // 2.2. Bir Tag'e Ait Tüm Post'ları Getir
        [HttpGet]
        public async Task<IActionResult> GetPostsForTag(Guid tagId)
        {
            var result = await _postTagService.GetPostsByTagIdAsync(tagId);
            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            return View("PostsByTag", result.Data);
        }

        // 2.3. Bir Post'a Çoklu Tag Ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTagsToPost(Guid postId, List<Guid> tagIds)
        {
            var result = await _postTagService.AddTagsToPostAsync(postId, tagIds);
            TempData["SuccessMessage"] = result.IsSucces ? "Etiketler başarıyla eklendi!" : result.Message;
            return RedirectToAction(nameof(Index));
        }

        // 2.4. Bir Post'tan Çoklu Tag Kaldır
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignTagsFromPost(Guid postId, List<Guid> tagIds)
        {
            var result = await _postTagService.RemoveTagsFromPostAsync(postId, tagIds);
            TempData["SuccessMessage"] = result.IsSucces ? "Seçili etiketler kaldırıldı!" : result.Message;
            return RedirectToAction(nameof(Index));
        }

        // 2.5. Bir Post'taki Belirli Tag Var mı Diye Kontrol Et
        [HttpGet]
        public async Task<IActionResult> IsTagAssignedToPost(Guid postId, Guid tagId)
        {
            var result = await _postTagService.CheckIfTagExistsInPostAsync(postId, tagId);
            return Json(new { Exists = result.Data });
        }

        // 2.6. Bir PostTag Kaydının Aktiflik Durumunu Değiştir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTagStatusInPost(Guid postTagId)
        {
            var result = await _postTagService.TogglePostTagActivationAsync(postTagId);
            TempData["SuccessMessage"] = result.IsSucces ? "Tag aktiflik durumu değiştirildi!" : result.Message;
            return RedirectToAction(nameof(Index));
        }

        // 2.7. Bir Post'un Tüm Tag'lerini Kaldır
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearTagsFromPost(Guid postId)
        {
            var result = await _postTagService.RemoveAllTagsFromPostAsync(postId);
            TempData["SuccessMessage"] = result.IsSucces ? "Tüm etiketler kaldırıldı!" : result.Message;
            return RedirectToAction(nameof(Index));
        }

        #endregion

    }
}
