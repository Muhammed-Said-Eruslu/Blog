using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Business.Services.PostServices;
using Business.Services.CommentsServices;
using Business.DTOs.PostDTOs;
using Business.DTOs.CommentsDTOs;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using AspNetCoreHero.ToastNotification.Abstractions;
using Infrastructure.Repositories.ContactRepository;
using Business.Services.ContactServices;
using Microsoft.AspNetCore.Identity;
using Business.Services.PhotoServices;
using Business.Helpers;

namespace UI.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly INotyfService _notfy;
        private readonly IContactService _contactService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IPhotoService _photoService;

        public HomeController(IPostService postService, ICommentService commentService, INotyfService notfy, IContactService contactService, UserManager<AppUser> userManager, IPhotoService photoService)
        {
            _postService = postService;
            _commentService = commentService;
            _notfy = notfy;
            _contactService = contactService;
            _userManager = userManager;
            _photoService = photoService;
        }

        /// <summary>
        /// Anasayfa: Arama, sayfalama ve popüler postları getirir
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index(string searchQuery, int pageNumber = 1, int pageSize = 6)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdClaim, out Guid userId); // Kullanıcı giriş yapmamışsa `Guid.Empty` olur

            var postsResult = await _postService.GetAllWithCountAsync(userId);
            var popularPostsResult = await _postService.GetPopularPostsAsync(3);

            if (!postsResult.IsSucces || postsResult.Data == null)
            {
                TempData["ErrorMessage"] = "Blog yazıları yüklenemedi.";
                return View(new PaginatedList<PostListDTO>(new List<PostListDTO>(), 0, pageNumber, pageSize));
            }

            var filteredPosts = postsResult.Data;
            var totalCount = filteredPosts.Count();
            var items = filteredPosts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.SearchQuery = searchQuery;
            ViewBag.PopularPosts = popularPostsResult.Data;

            return View(new PaginatedList<PostListDTO>(items, totalCount, pageNumber, pageSize));
        }


        /// <summary>
        /// Detay sayfası (Slug bazlı)
        /// </summary>
        // HomeController.cs - Details Metodu
        [HttpGet("Details/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var result = await _postService.GetPostWithIncludesAsync(slug);

            if (!result.IsSucces || result.Data == null)
            {
                TempData["ErrorMessage"] = "Gönderi bulunamadı.";
                return RedirectToAction("Index");
            }

            // Görüntülenme sayısını artır
            await _postService.IncreaseViewCountAsync(result.Data.Id);

            // PostDetailDTO'ya çevir
            var postDetailDto = result.Data.Adapt<PostDetailDTO>();

    //           postDetailDto.Comments = CommentMappingHelper.MapCommentsWithReplies(
    //    result.Data.Comments.ToList() // Burada tüm yorumlar gelmeli
    //);


            // İlişkili verileri doldur
            var relatedTagResult = await _postService.GetRelatedTagPostsAsync(result.Data.Id);
            var relatedCategoryResult = await _postService.GetRelatedCategoryPostsAsync(result.Data.Id);
            var popularPostsResult = await _postService.GetPopularPostsAsync(5);


            postDetailDto.RelatedTagPosts = relatedTagResult.Data?.Adapt<List<PostDetailDTO>>() ?? new List<PostDetailDTO>();
            postDetailDto.RelatedCategoryPosts = relatedCategoryResult.Data?.Adapt<List<PostDetailDTO>>() ?? new List<PostDetailDTO>();
            postDetailDto.PopularPosts = popularPostsResult.Data?.Adapt<List<PostDetailDTO>>() ?? new List<PostDetailDTO>();


            return View(postDetailDto);
        }

        [HttpPost("User/Post/AddComment")]
        public async Task<IActionResult> AddComment(CommentCreateDTO model)
        {
            // Sadece AJAX isteklerine cevap ver (SPA tarzı uygulamalar için)
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return isAjax
                    ? Json(new { success = false, message = "Kullanıcı bilgisi alınamadı." })
                    : RedirectToLogin(model.PostSlug);
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return isAjax
                    ? Json(new { success = false, message = "Kullanıcı bulunamadı." })
                    : RedirectToLogin(model.PostSlug);
            }

            // Modeli hazırla
            model.UserId = userId;
            model.AuthorName = user.UserName;
            model.AuthorProfileImage = user.ProfileImage ?? "/images/default-avatar.png";
            model.ParentCommentId = model.ParentCommentId == Guid.Empty ? null : model.ParentCommentId;

            // Validasyon
            if (!ModelState.IsValid)
            {
                return isAjax
                    ? Json(new { success = false, message = "Lütfen tüm alanları doldurun." })
                    : RedirectWithError(model.PostSlug, "Lütfen tüm alanları doldurun.");
            }

            var result = await _commentService.AddAsync(model);

            if (!result.IsSucces)
            {
                return isAjax
                    ? Json(new { success = false, message = result.Message })
                    : RedirectWithError(model.PostSlug, result.Message);
            }

            // Başarılı sonuç - AJAX isteklerinde yönlendirme yapma, sadece bilgi dön
            if (isAjax)
            {
                return Json(new
                {
                    success = true,
                    message = "Yorumunuz başarıyla eklendi!",
                    comment = new
                    {
                        id = result.Message,
                        authorName = model.AuthorName,
                        authorProfileImage = model.AuthorProfileImage,
                        content = model.Content,
                        parentCommentId = model.ParentCommentId,
                        createdAt = DateTime.Now.ToString("dd MMM yyyy HH:mm")
                    },
                    // Doğru URL formatını oluştur
                    redirectUrl = $"/Details/{model.PostSlug}"
                });
            }

            // Normal form post için yönlendirme
            TempData["SuccessMessage"] = "Yorumunuz başarıyla eklendi!";
            return Redirect($"/Details/{model.PostSlug}");
        }




        /// <summary>
        /// Beğenme (Ajax)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LikePost(Guid postId)
        {
            try
            {
                if (postId == Guid.Empty)
                    return Json(new { success = false, message = "Geçersiz Post ID!" });

                // Like artır
                var result = await _postService.IncreaseLikeCountAsync(postId);
                if (!result.IsSucces)
                    return Json(new { success = false, message = result.Message });

                // Güncellenmiş postu al
                var updatedPost = await _postService.GetByIdAsync(postId);
                if (updatedPost.Data == null)
                    return Json(new { success = false, message = "Gönderi bulunamadı!" });

                // Yeni likeCount'u dön
                return Json(new { success = true, likeCount = updatedPost.Data.LikeCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        /// <summary>
        /// Görüntülenme sayısını artırma (Ajax)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> IncreaseViewCount(Guid postId)
        {
            var result = await _postService.IncreaseViewCountAsync(postId);
            if (!result.IsSucces)
                return Json(new { success = false });

            var updatedPost = await _postService.GetByIdAsync(postId);
            return Json(new { success = true, viewCount = updatedPost.Data.ViewCount });
        }

        /// <summary>
        /// Canlı Arama (Autocomplete) - Ajax GET
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<JsonResult> GetAutocompleteResults(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var postsResult = await _postService.GetAllWithCountAsync(term);

            var list = postsResult.Data
                .OrderByDescending(p => p.CreatedDate)
                .Take(5)
                .Select(p => new
                {
                    slug = p.Slug,
                    title = p.Title,
                    image = p.FeaturedImage, // Resim URL'si
                    category = p.CategoryName, // Kategori adı
                    date = p.CreatedDate.ToString("dd MMM yyyy") // Tarih
                }).ToList();

            return Json(list);
        }

        [Authorize]
        [HttpGet("LikedPosts")]
        public async Task<IActionResult> LikedPosts()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                TempData["ErrorMessage"] = "Lütfen giriş yapınız!";
                return RedirectToAction("Index", "Home");
            }

            // Kullanıcının beğendiği postları Include() ile çekiyoruz.
            var user = await _postService.GetUserWithLikedPostsAsync(userId);

            if (user == null || user.LikedPosts == null || !user.LikedPosts.Any())
            {
                TempData["InfoMessage"] = "Henüz beğendiğiniz bir gönderi bulunmuyor!";
                return View(new List<PostListDTO>());
            }

            // Null kontrolü ile Post nesnesini güvenli bir şekilde çekiyoruz
            var likedPosts = user.LikedPosts
                .Where(lp => lp.Post != null) // `null` kontrolü
                .Select(lp => lp.Post.Adapt<PostListDTO>())
                .ToList();

            return View(likedPosts);
        }



        [HttpPost]
        public async Task<IActionResult> ToggleLike(Guid postId)
        {
            if (!User.Identity.IsAuthenticated)
                return Json(new { success = false, message = "Önce giriş yapmalısınız!" });

            // Kullanıcı ID'yi al
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Json(new { success = false, message = "Kullanıcı kimliği bulunamadı, lütfen tekrar giriş yapın!" });
            }

            // Kullanıcının postu beğenip beğenmediğini kontrol et
            var result = await _postService.ToggleLikeAsync(userId, postId);

            if (!result.IsSucces)
                return Json(new { success = false, message = result.Message });

            return Json(new
            {
                success = true,
                likeCount = result.Data.LikeCount,
                isLiked = result.Data.LikedUsers.Any(l => l.UserId == userId) // Beğeni durumu
            });
        }

        [HttpGet("Hakkımda")]
        public async Task<IActionResult> About()
        {
            return View();
        }

        [HttpGet("İletişim")]
        public async Task<IActionResult> Contact()
        {
            return View();
        }

        [HttpPost("İletişim")]
        public async Task<IActionResult> Contact(Contact model)
        {
            Console.WriteLine($"Gelen ID: {model.Id}");
            if (model == null)
            {
                _notfy.Error("Lütfen geçerli bir mesaj giriniz.");
                return View();
            }

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
            }

            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"Hata: {entry.Key} - {error.ErrorMessage}");
                    }
                }

                _notfy.Error("Lütfen tüm alanları doğru bir şekilde doldurun.");
                return View(model);
            }



            try
            {
                await _contactService.AddAsync(model);
                _notfy.Success("Mesajınız başarıyla gönderildi!");
                return RedirectToAction("Contact");
            }
            catch (Exception ex)
            {
                _notfy.Error("Mesaj gönderilirken bir hata oluştu. Hata: " + ex.Message);
                return View(model);
            }
        }
        [HttpGet("Fotoğraflar")]
        public async Task<IActionResult> Photo()
        {
            var photos = await _photoService.GetAllPhotosAsync();
            return View(photos);
        }
        private IActionResult RedirectToLogin(string slug)
        {
            TempData["ErrorMessage"] = "Lütfen giriş yapınız.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Post", new { slug }) });
        }

        private IActionResult RedirectWithError(string slug, string errorMessage)
        {
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction("Details", new { slug });
        }
    }
}
