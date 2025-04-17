using Business.DTOs.CategoryDTOs;
using Business.DTOs.CommentsDTOs;
using Business.DTOs.PostDTOs;
using Business.DTOs.PostTagDTOs;
using Business.DTOs.UsersDTOs;
using Business.Services.CategoryServices;
using Business.Services.PostServices;
using Business.Services.TagServices;
using Domain.Entites;
using Domain.Enums;
using Domain.Utilities.Concretes;
using Domain.Utilities.Interfaces;
using Mapster;  // Mapster'ı ekliyoruz
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;
        private readonly ITagService _tagService;
        private readonly UserManager<AppUser> _userManager;

        public PostController(IPostService postService, ICategoryService categoryService, ITagService tagService, UserManager<AppUser> userManager)
        {
            _postService = postService;
            _categoryService = categoryService;
            _tagService = tagService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _postService.GetAllAsync();
            var categories = await _categoryService.GetAllAsync();

            if (result.IsSucces)
            {
                ViewBag.Categories = categories.Data.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

                // Slug için yazıları ViewBag içinde gönderiyoruz
                ViewBag.Posts = result.Data.Select(p => new
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug
                }).ToList();

                return View(result.Data);
            }

            TempData["ErrorMessage"] = "Yazılar alınırken bir hata oluştu.";
            return View(result.Data);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Kategorileri asenkron çekiyoruz ve .Data üzerinden listeye erişiyoruz.
            var categoryResult = await _categoryService.GetAllAsync();
            var categoryItems = categoryResult.Data.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            // Identity üzerinden kullanıcıları çekiyoruz (SelectListItem olarak).
            var userItems = await _userManager.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UserName  // veya FullName gibi bir property
                })
                .ToListAsync();

            // Etiketleri asenkron çekip .Data üzerinden listeye erişiyoruz.
            var tagResult = await _tagService.GetAllAsync();
            var tagItems = tagResult.Data.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
            }).ToList();

            // ViewBag'e atama yapıyoruz.
            ViewBag.Categories = new SelectList(categoryItems, "Value", "Text");
            ViewBag.Users = new SelectList(userItems, "Value", "Text");
            ViewBag.Tags = new MultiSelectList(tagItems, "Value", "Text");

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(PostCreateDTO postCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(postCreateDTO);
            }

            // Video dosyasını kaydetme işlemi
            if (postCreateDTO.VideoFile != null && postCreateDTO.VideoFile.Length > 0)
            {
                var videoUploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "videos");
                if (!Directory.Exists(videoUploadsFolder))
                    Directory.CreateDirectory(videoUploadsFolder);

                var uniqueVideoName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(postCreateDTO.VideoFile.FileName);
                var videoPath = Path.Combine(videoUploadsFolder, uniqueVideoName);

                using (var stream = new FileStream(videoPath, FileMode.Create))
                {
                    await postCreateDTO.VideoFile.CopyToAsync(stream);
                }

                // Sadece yolu DTO'ya kaydet
                postCreateDTO.VideoUrl = "/uploads/videos/" + uniqueVideoName;
            }

            // Öne çıkan resmi kaydetme işlemi
            if (postCreateDTO.FeaturedImageFile != null && postCreateDTO.FeaturedImageFile.Length > 0)
            {
                var imageUploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "images");
                if (!Directory.Exists(imageUploadsFolder))
                    Directory.CreateDirectory(imageUploadsFolder);

                var uniqueImageName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(postCreateDTO.FeaturedImageFile.FileName);
                var imagePath = Path.Combine(imageUploadsFolder, uniqueImageName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await postCreateDTO.FeaturedImageFile.CopyToAsync(stream);
                }

                // Sadece yolu DTO'ya kaydet
                postCreateDTO.FeaturedImage = "/uploads/images/" + uniqueImageName;
            }

            var result = await _postService.AddAsync(postCreateDTO);
            if (result.IsSucces)
            {
                TempData["SuccessMessage"] = "Yazı başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Yazı oluşturulurken bir hata oluştu: " + result.Message;
            return View(postCreateDTO);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _postService.GetPostDetailsWithIncludesAsync(id);
            if (!result.IsSucces || result.Data == null)
            {
                TempData["ErrorMessage"] = "Düzenlenecek yazı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var updateDTO = result.Data.Adapt<PostUpdateDTO>();

            // TagIds null ise boş liste kullan
            updateDTO.TagIds = result.Data.TagIds ?? new List<Guid>();

            var users = await _userManager.Users.ToListAsync();
            var categories = await _categoryService.GetAllAsync();
            var tags = await _tagService.GetAllAsync();

            ViewBag.Users = new SelectList(users, "Id", "UserName", updateDTO.UserId);
            ViewBag.Categories = new SelectList(categories.Data, "Id", "Name", updateDTO.CategoryId);
            ViewBag.Tags = new MultiSelectList(tags.Data, "Id", "Name", updateDTO.TagIds);

            return View(updateDTO);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(PostUpdateDTO postUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                // Hata durumunda verileri tekrar yükle
                var users = await _userManager.Users.ToListAsync();
                var categories = await _categoryService.GetAllAsync();
                var tags = await _tagService.GetAllAsync();

                ViewBag.Users = new SelectList(users, "Id", "UserName", postUpdateDTO.UserId);
                ViewBag.Categories = new SelectList(categories.Data, "Id", "Name", postUpdateDTO.CategoryId);
                ViewBag.Tags = new MultiSelectList(tags.Data, "Id", "Name", postUpdateDTO.TagIds);

                return View(postUpdateDTO);
            }

            // Eğer yeni bir dosya yüklenmişse, dosyayı kaydet ve FeaturedImage property'sini güncelle
            if (postUpdateDTO.FeaturedImageFile != null && postUpdateDTO.FeaturedImageFile.Length > 0)
            {
                // Örneğin, wwwroot/uploads klasörüne kaydedelim
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + postUpdateDTO.FeaturedImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await postUpdateDTO.FeaturedImageFile.CopyToAsync(fileStream);
                }

                // Yeni dosya yolunu ayarla (örneğin, "/uploads/uniqueFileName")
                postUpdateDTO.FeaturedImage = "/uploads/" + uniqueFileName;
            }

            var result = await _postService.UpdateAsync(postUpdateDTO);

            if (result.IsSucces)
            {
                TempData["SuccessMessage"] = "Yazı başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            // Hata durumunda verileri tekrar yükle
            var usersList = await _userManager.Users.ToListAsync();
            var categoriesList = await _categoryService.GetAllAsync();
            var tagsList = await _tagService.GetAllAsync();

            ViewBag.Users = new SelectList(usersList, "Id", "UserName", postUpdateDTO.UserId);
            ViewBag.Categories = new SelectList(categoriesList.Data, "Id", "Name", postUpdateDTO.CategoryId);
            ViewBag.Tags = new MultiSelectList(tagsList.Data, "Id", "Name", postUpdateDTO.TagIds);

            TempData["ErrorMessage"] = "Yazı güncellenirken bir hata oluştu: " + result.Message;
            return View(postUpdateDTO);
        }



        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _postService.DeleteAsync(id);
            if (result.IsSucces)
            {
                TempData["SuccessMessage"] = "Yazı başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Yazı silinemedi. Lütfen önce yazıya bağlı yorumları kaldırın.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetPostsByCategoryId(Guid categoryId)
        {
            var result = await _postService.GetPostsByCategoryIdAsync(categoryId);
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetPostsByUserId(Guid userId)
        {
            var result = await _postService.GetPostsByUserIdAsync(userId);
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> SearchPosts(string searchTerm)
        {
            var result = await _postService.SearchPostsAsync(searchTerm);
            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePostStatus(Guid postId, Status status)
        {
            var result = await _postService.UpdatePostStatusAsync(postId, status);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddCommentToPost(Guid postId, CommentCreateDTO commentCreateDTO)
        {
            var result = await _postService.AddCommentToPostAsync(postId, commentCreateDTO);
            return RedirectToAction(nameof(GetPostDetails), new { postId });
        }

        [HttpPost]
        public async Task<IActionResult> AddTagsToPost(Guid postId, List<Guid> tagIds)
        {
            var result = await _postService.AddTagsToPostAsync(postId, tagIds);

            if (result.IsSucces)
            {
                TempData["SuccessMessage"] = "Etiketler başarıyla eklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Etiketler eklenirken bir hata oluştu: " + result.Message;
            }

            return RedirectToAction(nameof(GetPostDetails), new { postId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTagsFromPost(Guid postId, List<Guid> tagIds)
        {
            var result = await _postService.RemoveTagsFromPostAsync(postId, tagIds);
            return RedirectToAction(nameof(GetPostDetails), new { postId });
        }

        [HttpGet]
        public async Task<IActionResult> GetPostDetails(Guid postId)
        {
            // Post detaylarını al
            var result = await _postService.GetPostDetailsWithIncludesAsync(postId);

            if (!result.IsSucces || result.Data == null)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            // Tüm etiketleri al
            var tagsResult = await _tagService.GetAllAsync();

            // Etiketleri ViewBag'e ekle
            ViewBag.Tags = tagsResult.Data;

            return View(result.Data);
        }


        [HttpGet]
        public async Task<IActionResult> GetPopularPosts(int count)
        {
            var result = await _postService.GetPopularPostsAsync(count);
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetPostBySlug(string slug)
        {
            var result = await _postService.GetPostBySlugAsync(slug);
            if (result.Data == null)
            {
                return NotFound();
            }

            var postDetailDTO = result.Data.Adapt<PostDetailDTO>();

            // Yazar bilgisi
            var user = await _userManager.FindByIdAsync(result.Data.UserId.ToString());
            postDetailDTO.Author = user != null ? user.Adapt<UserDTO>() : null;
            // ViewBag ile ekstra bilgiler
            ViewBag.Categories = (await _categoryService.GetAllAsync()).Data;
            ViewBag.Tags = (await _tagService.GetAllAsync()).Data;
            ViewBag.CurrentUser = postDetailDTO.Author;

            return View(postDetailDTO);
        }

        [HttpPost]
        public async Task<IActionResult> BulkDeletePosts(List<Guid> postIds)
        {
            var result = await _postService.BulkDeletePostsAsync(postIds);
            if (result.IsSucces)
            {
                TempData["SuccessMessage"] = "Seçilen yazılar başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Yazılar silinirken hata oluştu.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetPostStatistics()
        {
            var result = await _postService.GetPostStatisticsAsync();
            return View(result.Data);
        }
        [HttpPost]
        public async Task<IActionResult> UploadVideo(Guid postId, IFormFile videoFile)
        {
            if (videoFile == null || videoFile.Length == 0)
            {
                return Json(new { success = false, message = "Lütfen geçerli bir video dosyası seçin." });
            }

            // Video dosyasını kaydet
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "videos");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + videoFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await videoFile.CopyToAsync(fileStream);
            }

            // Video URL'sini güncelle
            var videoUrl = "/uploads/videos/" + uniqueFileName;
            var result = await _postService.UpdateVideoUrlAsync(postId, videoUrl);

            if (result.IsSucces)
            {
                return Json(new { success = true, videoUrl = videoUrl });
            }
            else
            {
                return Json(new { success = false, message = "Video yüklenirken bir hata oluştu: " + result.Message });
            }
        }
    }
}
