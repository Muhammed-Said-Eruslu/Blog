using Business.DTOs.CommentsDTOs;
using Business.Services.PostServices;
using Domain.Utilities.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;

namespace UI.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IPostService _postService;

        public CommentController(ICommentService commentService, IPostService postService)
        {
            _commentService = commentService;
            _postService = postService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _commentService.GetAllAsync();
            return View(result.Data);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _commentService.GetByIdAsync(id);
            if (!result.IsSucces) return NotFound();
            return View(result.Data);
        }

        public async Task<IActionResult> Create()
        {
            var postsResult = await _postService.GetAllAsync(); // Async çağrıyı bekle

            if (!postsResult.IsSucces || postsResult.Data == null)
            {
                ModelState.AddModelError(string.Empty, "Yazılar yüklenirken bir hata oluştu.");
                ViewBag.Posts = new List<SelectListItem>(); // Boş bir liste atayarak hata önlenir
            }
            else
            {
                ViewBag.Posts = postsResult.Data.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Title
                }).ToList();
            }

            return View(new CommentCreateDTO());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CommentCreateDTO commentCreateDTO)
        {
            if (!ModelState.IsValid || commentCreateDTO.PostId == Guid.Empty)
            {
                ModelState.AddModelError("PostId", "Lütfen bir yazı seçin!");

                var postsResult = await _postService.GetAllAsync(); // Yine async çağrıyı bekle
                if (!postsResult.IsSucces || postsResult.Data == null)
                {
                    ViewBag.Posts = new List<SelectListItem>();
                }
                else
                {
                    ViewBag.Posts = postsResult.Data.Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Title
                    }).ToList();
                }

                return View(commentCreateDTO);
            }

            var result = await _commentService.AddAsync(commentCreateDTO);
            if (result.IsSucces)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(commentCreateDTO);
        }



        public async Task<IActionResult> Edit(Guid id)
        {
            var commentResult = await _commentService.GetByIdAsync(id);
            if (!commentResult.IsSucces) return NotFound();

            var updateDto = new CommentUpdateDTO
            {
                Id = id,
                Content = commentResult.Data.Content, // Yorum içeriği
                PostId = commentResult.Data.PostId,   // Yorumun bağlı olduğu yazı ID'si
                UserId = commentResult.Data.UserId,   // Yorum yapan kullanıcının ID'si
                AuthorName = commentResult.Data.AuthorName, // Yazarın adı
                AuthorEmail = commentResult.Data.AuthorEmail, // Yazarın e-posta adresi
                ParentCommentId = commentResult.Data.ParentCommentId // Üst yorum (eğer varsa)
            };

            return View(updateDto);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(CommentUpdateDTO commentUpdateDTO)
        {
            if (!ModelState.IsValid) return View(commentUpdateDTO);

            var result = await _commentService.UpdateAsync(commentUpdateDTO);
            if (result.IsSucces) return RedirectToAction(nameof(Details), new { id = commentUpdateDTO.Id });

            ModelState.AddModelError(string.Empty, result.Message);
            return View(commentUpdateDTO);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _commentService.DeleteAsync(id);
            if (result.IsSucces)
                return Json(new { success = true, message = result.Message });
            else
                return Json(new { success = false, message = result.Message });
        }

        public async Task<IActionResult> ByPost(Guid postId)
        {
            var result = await _commentService.GetByPostIdAsync(postId);
            return View(result.Data);
        }

        public async Task<IActionResult> Replies(Guid parentCommentId)
        {
            var result = await _commentService.GetRepliesAsync(parentCommentId);
            return View(result.Data);
        }
        public async Task<IActionResult> Comments()
        {
            // Veritabanından asenkron olarak yorumları alıyoruz
            var result = await _commentService.GetAllAsync();

            if (result.IsSucces)
            {
                // Türkiye saatine dönüştürme işlemi
                foreach (var comment in result.Data)
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                    comment.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(comment.CreatedDate, timeZone);
                }

                return View(result.Data);  // Yorumları view'a gönderiyoruz
            }

            // Eğer hata varsa, hata mesajını döndür
            TempData["Error"] = result.Message;
            return View(new List<CommentListDTO>());
        }

        public async Task<IActionResult> ByUser(Guid userId)
        {
            var result = await _commentService.GetByUserIdAsync(userId);

            if (!result.IsSucces)
            {
                ViewBag.Message = result.Message; // Hata mesajını ViewBag ile gönder
                return View(new List<CommentListDTO>()); // Boş liste döndür
            }

            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleVisibility(Guid commentId)
        {
            var result = await _commentService.ToggleCommentVisibilityAsync(commentId);
            return RedirectToAction(nameof(Details), new { id = commentId });
        }

        public async Task<IActionResult> PaginatedByPost(Guid postId, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _commentService.GetPaginatedByPostAsync(postId, pageNumber, pageSize);
            return View(result.Data);
        }

        public async Task<IActionResult> CommentCount(Guid postId)
        {
            var result = await _commentService.GetCommentCountByPostAsync(postId);
            return View(result.Data);
        }

        public async Task<IActionResult> CheckUserCommented(Guid postId, Guid userId)
        {
            var result = await _commentService.CheckIfUserCommentedAsync(postId, userId);
            return View(result.Data);
        }

        public async Task<IActionResult> RecentComments(int count = 5)
        {
            var result = await _commentService.GetRecentCommentsAsync(count);
            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> FlagInappropriate(Guid commentId)
        {
            var result = await _commentService.FlagCommentAsInappropriateAsync(commentId);
            return RedirectToAction(nameof(Details), new { id = commentId });
        }

        public async Task<IActionResult> WithReplies(Guid commentId)
        {
            var result = await _commentService.GetWithRepliesAsync(commentId);
            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAuthorInfo(Guid commentId, string authorName, string authorEmail)
        {
            var result = await _commentService.UpdateAuthorInfoAsync(commentId, authorName, authorEmail);
            return RedirectToAction(nameof(Details), new { id = commentId });
        }
    }
}