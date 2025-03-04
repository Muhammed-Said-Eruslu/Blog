using Business.DTOs.UserDTOs;
using Business.Services.AccountService;
using Domain.Entites;

using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UI.Areas.User.Models.AccountVMs;
using static Infrastructure.Helpers.NotyfHelper;

namespace UI.Areas.User.Controllers
{
    [Area("User")]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(IAccountService accountService, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _accountService = accountService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _accountService.FindByEmail(model.Email);
            if (user == null)
            {
                ErrorNotyf("E-posta adresi veya şifre hatalı!");
                return View(model);
            }

            // Eğer hesap doğrulanmamışsa giriş yapamaz
            if (!user.EmailConfirmed)
            {
                ErrorNotyf("E-posta adresinizi doğrulamadığınız için giriş yapamazsınız!");
                return RedirectToAction("EmailConfirm");
            }

            var result = await _accountService.LoginAsync(model.Email, model.Password, model.RememberMe);
            if (result.IsSucces)
            {
                SuccessNotyf("Başarıyla giriş yaptınız.");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ErrorNotyf(result.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // 🛑 1. Kullanıcı daha önce kayıtlı mı kontrol et
                var existingUser = await _accountService.FindByEmail(model.Email);
                if (existingUser != null)
                {
                    ErrorNotyf("Bu e-posta adresi ile kayıtlı bir kullanıcı zaten var.");
                    return View(model);
                }

                // 🛑 2. E-Posta doğrulama kodu oluştur ve gönder
                var confirmCode = await _accountService.SendConfirmMail(model.Email);
                if (string.IsNullOrEmpty(confirmCode) || confirmCode == "0")
                {
                    ErrorNotyf("E-posta gönderilirken bir hata oluştu.");
                    return View(model);
                }

                // 🛑 3. Kullanıcı bilgilerini TempData'da sakla
                TempData["ConfirmCode"] = confirmCode;
                TempData["RegisterData"] = JsonConvert.SerializeObject(model);

                SuccessNotyf("E-posta adresinizi doğrulayın.");
                return RedirectToAction("EmailConfirm");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult EmailConfirm()
        {
            if (TempData["ConfirmCode"] == null || TempData["RegisterData"] == null)
            {
                ErrorNotyf("Doğrulama süresi doldu. Lütfen tekrar kayıt olun.");
                return RedirectToAction("Register");
            }

            var registerVM = JsonConvert.DeserializeObject<RegisterVM>(TempData["RegisterData"].ToString());
            TempData.Keep("RegisterData"); // TempData'yı POST metodu için sakla

            SendMailVM sendMailVM = new SendMailVM
            {
                ConfirmCode = TempData["ConfirmCode"].ToString(),
                RegisterVM = registerVM,
            };

            return View(sendMailVM);
        }

        [HttpPost]
        public async Task<IActionResult> EmailConfirm(SendMailVM model)
        {
            if (!ModelState.IsValid || model.UserCode != model.ConfirmCode)
            {
                ErrorNotyf("Doğrulama kodu hatalı veya form eksik.");
                return View(model);
            }

            try
            {
                // Kullanıcı oluştur
                var appUser = new AppUser
                {
                    UserName = model.RegisterVM.Email,
                    Email = model.RegisterVM.Email,
                    EmailConfirmed = true, // Kullanıcı doğrulandı
                    Role = Roles.User,
                    FullName = $"{model.RegisterVM.FirstName} {model.RegisterVM.LastName}",
                    NormalizedEmail = model.RegisterVM.Email.ToUpperInvariant(),
                    NormalizedUserName = $"{model.RegisterVM.FirstName}  {model.RegisterVM.LastName}",
                };

                // Kullanıcıyı oluştur
                var createUserResult = await _userManager.CreateAsync(appUser, model.RegisterVM.Password);
                if (!createUserResult.Succeeded)
                {
                    ErrorNotyf("Kullanıcı oluşturulamadı: " + string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
                    return RedirectToAction("Register");
                }

                // 🛑 `NormalizedEmail` ve `NormalizedUserName` değerlerini set et
                appUser.NormalizedUserName = $"{model.RegisterVM.FirstName} {model.RegisterVM.LastName}".ToUpperInvariant();
                appUser.NormalizedEmail = appUser.Email.ToUpperInvariant();
                await _accountService.UpdateUserAsync(appUser);

                //// 🛑 Eğer `UserManager.UpdateAsync` başarısız olursa hata mesajı döndür
                //var updatedUser = await _accountService.FindByEmail(appUser.Email );
                //if (updatedUser.Email == null)
                //{
                //    ErrorNotyf("NormalizedEmail güncellenemedi!");
                //    return View(model);
                //}

                // Başarı mesajı göster ve yönlendir
                SuccessNotyf("E-Posta doğrulandı ve hesabınız oluşturuldu! Şimdi giriş yapabilirsiniz.");
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ErrorNotyf("Bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }



        public async Task<IActionResult> Logout()
        {
            var result = await _accountService.LogoutAsync();
            if (!result.IsSucces)
            {
                ErrorNotyf(result.Message);
            }
            SuccessNotyf(result.Message);
            return RedirectToAction("Login");
        }
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new UserProfileDTO
            {
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                ProfileImage = user.ProfileImage ?? "default-avatar.jpg"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserProfileDTO model)
        {
            if (!ModelState.IsValid) return View("Profile", model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.UserName = model.UserName;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profiliniz güncellendi!";
                return RedirectToAction("Profile");
            }

            TempData["Error"] = "Güncelleme başarısız!";
            return View("Profile", model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "Şifreler eşleşmiyor!";
                return RedirectToAction("Profile");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);
            if (changePasswordResult.Succeeded)
            {
                TempData["Success"] = "Şifreniz başarıyla değiştirildi!";
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Profile");
            }

            TempData["Error"] = "Şifre değiştirme başarısız!";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadProfileImage(IFormFile ProfileImage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                // Uzantıyı al
                var extension = Path.GetExtension(ProfileImage.FileName);

                // Kendimize özel bir dosya adı oluşturalım
                var fileName = $"{user.Id}_{Guid.NewGuid()}{extension}";

                // Kaydedilecek fiziksel yol
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatar", fileName);

                // Dosyayı kopyala
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }

                // Kullanıcının profil resmini güncelle
                user.ProfileImage = fileName;
                await _userManager.UpdateAsync(user);

                TempData["Success"] = "Profil resmi başarıyla güncellendi!";
            }
            else
            {
                TempData["Error"] = "Lütfen geçerli bir resim dosyası seçin.";
            }

            return RedirectToAction("Profile");
        }

    }
}
