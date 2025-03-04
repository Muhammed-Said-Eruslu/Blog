using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Business.Services.PostServices;
using Business.Services.CommentsServices;
using Business.DTOs.PostDTOs;
using Business.DTOs.CommentsDTOs;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Domain.Entites;

namespace UI.Areas.User.Controllers
{
    [Area("User")]

    public class HomeController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;

        public HomeController(IPostService postService, ICommentService commentService)
        {
            _postService = postService;
            _commentService = commentService;
        }
        [HttpGet("")]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 6)
        {
            var result = await _postService.GetAllWithCountAsync(); // Doğru metodu çağırdık

            if (!result.IsSucces || result.Data == null)
            {
                TempData["ErrorMessage"] = "Blog yazıları yüklenemedi.";
                return View(new PaginatedList<PostListDTO>(new List<PostListDTO>(), 0, pageNumber, pageSize));
            }

            var count = result.Data.Count();
            var items = result.Data.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return View(new PaginatedList<PostListDTO>(items, count, pageNumber, pageSize));
        }

        [HttpGet("Details")]

        public async Task<IActionResult> Details(string slug)
        {
            var result = await _postService.GetPostWithIncludesAsync<Post>(
                slug,
                query => query
                    .Include(p => p.PostTag).ThenInclude(pt => pt.Tag)
                    .Include(p => p.Comments)
            );

            if (!result.IsSucces || result.Data == null)
            {
                TempData["ErrorMessage"] = "Gönderi bulunamadı.";
                return RedirectToAction("Index");
            }

            await _postService.IncreaseViewCountAsync(result.Data.Id);
            var postDetailDto = result.Data.Adapt<PostDetailDTO>();

            return View(postDetailDto);
        }




        [HttpPost("User/Post/AddComment")]
        public async Task<IActionResult> AddComment(CommentCreateDTO model)
        {
            model.AuthorEmail = model.AuthorName;
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen tüm alanları doldurun.";
                return RedirectToAction("Details", new { id = model.PostId });
            }

            var result = await _commentService.AddAsync(model);

            if (!result.IsSucces)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = "Yorumunuz başarıyla eklendi!";
            }

            return RedirectToAction("Details", new { id = model.PostId });
        }
        [HttpPost]
        public async Task<IActionResult> LikePost(Guid postId)
        {
            Console.WriteLine("LikePost method called with postId: " + postId); // Log ekledik
            try
            {
                if (postId == Guid.Empty)
                    return Json(new { success = false, message = "Geçersiz Post ID!" });

                var result = await _postService.IncreaseLikeCountAsync(postId);
                if (!result.IsSucces)
                    return Json(new { success = false, message = result.Message });

                var updatedPost = await _postService.GetByIdAsync(postId);
                if (updatedPost.Data == null)
                    return Json(new { success = false, message = "Gönderi bulunamadı!" });

                return Json(new { success = true, likeCount = updatedPost.Data.LikeCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata oluştu: " + ex.Message); // Log ekledik
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> IncreaseViewCount(Guid postId)
        {
            var result = await _postService.IncreaseViewCountAsync(postId);
            if (!result.IsSucces)
                return Json(new { success = false });

            var updatedPost = await _postService.GetByIdAsync(postId);
            return Json(new { success = true, viewCount = updatedPost.Data.ViewCount });
        }



    }
}

