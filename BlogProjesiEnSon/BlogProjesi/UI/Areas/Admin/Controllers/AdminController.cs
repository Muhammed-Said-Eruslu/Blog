using Business.Services.PostServices;
using Business.Services.CategoryServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Domain.Entites; // AppUser
using System.Threading.Tasks;
using System.Linq;
using UI.Areas.Admin.Models;
using UI.Areas.Admin.Models.VMs.DashboardVM;
using Microsoft.AspNetCore.Authorization;
using UI.Areas.Admin.Models.VMs.AdminVM;
using Domain.Enums;
using Business.Services.AccountService; // DashboardViewModel

namespace UI.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;
        private readonly ICommentService _commentService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IAccountService _accountService;


        public AdminController(IPostService postService,
                               ICategoryService categoryService,
                               ICommentService commentService,
                               UserManager<AppUser> userManager,
                               SignInManager<AppUser> signInManager,
                               IAccountService accountService)
        {
            _postService = postService;
            _categoryService = categoryService;
            _commentService = commentService;
            _userManager = userManager;
            _signInManager = signInManager;
            _accountService = accountService;
        }

        public async Task<IActionResult> Index()
        {
            // 1) Servis çağır: IDataResult<List<PostListDTO>>
            var allPostsResult = await _postService.GetAllAsync();
            var allCategoriesResult = await _categoryService.GetAllAsync();
            var allCommentsResult = await _commentService.GetAllAsync();

            // 2) İstediğimiz asıl listeler:
            var allPosts = allPostsResult.Data;         // List<PostListDTO>
            var allCategories = allCategoriesResult.Data;   // List<CategoryListDTO> (varsayım)
            var allComments = allCommentsResult.Data;   // List<CommentListDTO>  (varsayım)
            var totalUsers = _userManager.Users.Count(); // Identity üzerinden kullanıcı sayısı

            // Hata veya null kontrolleri
            if (!allPostsResult.IsSucces || allPosts == null)
                return View("Error"); // veya istediğin hata handling
            if (!allCategoriesResult.IsSucces || allCategories == null)
                return View("Error");
            if (!allCommentsResult.IsSucces || allComments == null)
                return View("Error");

            // 3) Artık allPosts, allCategories, allComments listelerini LINQ ile kullanabilirsin
            var newPostsLast24Hours = allPosts.Count(p => p.CreatedDate >= System.DateTime.Now.AddDays(-1));
            var newCommentsLast24Hours = allComments.Count(c => c.CreatedDate >= System.DateTime.Now.AddDays(-1));

            var popularPosts = allPosts
                .OrderByDescending(p => p.ViewCount)
                .Take(3)
                .Select(p => p.Title)
                .ToList();

            var recentPosts = allPosts
                .OrderByDescending(p => p.CreatedDate)
                .Take(3)
                .Select(p => p.Title)
                .ToList();

            // Aylık yazı istatistiği (12 ay)
            var monthlyData = new int[12];
            foreach (var post in allPosts)
            {
                int monthIndex = post.CreatedDate.Month - 1; // 0..11
                monthlyData[monthIndex]++;
            }

            // 4) ViewModel'i doldur
            var model = new DashboardViewModel
            {
                TotalPosts = allPosts.Count,
                TotalCategories = allCategories.Count,
                TotalComments = allComments.Count,
                TotalUsers = totalUsers,

                NewPostsLast24Hours = newPostsLast24Hours,
                NewCommentsLast24Hours = newCommentsLast24Hours,

                PopularPosts = popularPosts,
                RecentPosts = recentPosts,
                MonthlyPostCounts = monthlyData
            };

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 👉 Servisten kullanıcıyı çek
            var user = await _accountService.FindByEmail(model.Email);

            // 👉 Kullanıcı yoksa veya admin değilse hata
            if (user == null || user.Role != Domain.Enums.Roles.Admin)
            {
                ModelState.AddModelError("", "Geçersiz kullanıcı veya yetki yok.");
                return View(model);
            }

            // 👉 Şifre kontrolü + giriş
            var result = await _accountService.LoginAsync(model.Email, model.Password, model.RememberMe);
            if (result.IsSucces)
            {
                return RedirectToAction("Index", "Admin", new { area = "Admin" });
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }


    }
}
