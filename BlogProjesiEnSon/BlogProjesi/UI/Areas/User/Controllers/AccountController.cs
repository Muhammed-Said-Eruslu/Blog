using AspNetCoreHero.ToastNotification.Abstractions;
using Business.DTOs.UserDTOs;
using Business.Services.AccountService;
using Domain.Entites;

using Domain.Enums;
using Infrastructure.Repositories.MailRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Newtonsoft.Json;
using System.Security.Claims;
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
        private readonly INotyfService _notyf;
        private readonly IAsyncSendMailRepository _mailService;

        public AccountController(IAccountService accountService, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, INotyfService notyf, IAsyncSendMailRepository mailService)
        {
            _accountService = accountService;
            _userManager = userManager;
            _signInManager = signInManager;
            _notyf = notyf;
            _mailService = mailService;
        }
        [HttpGet]
        [Route("giris-yap")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Route("giris-yap")]
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
        [Route("kayıt-ol")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Route("kayıt-ol")]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {

                var existingUser = await _accountService.FindByEmail(model.Email);
                if (existingUser != null)
                {
                    ErrorNotyf("Bu e-posta adresi ile kayıtlı bir kullanıcı zaten var.");
                    return View(model);
                }


                var confirmCode = await _accountService.SendConfirmMail(model.Email);
                if (string.IsNullOrEmpty(confirmCode) || confirmCode == "0")
                {
                    ErrorNotyf("E-posta gönderilirken bir hata oluştu.");
                    return View(model);
                }


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
            TempData.Keep("RegisterData");

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
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                // Resmi kaydet
                var fileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(ProfileImage.FileName)}"; // ✅ Uzantıyı ekledik
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatar", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }

                // Veritabanını güncelle
                user.ProfileImage = fileName;
                await _userManager.UpdateAsync(user);
                TempData["Success"] = "Profil resmi güncellendi!";
            }
            else
            {
                TempData["Error"] = "Geçersiz dosya!";
            }

            return RedirectToAction("Profile");
        }


        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            // Geri dönüş adresini açık ve net olarak belirtiyoruz (area dahil!)
            string redirectUrl = Url.Action("GoogleResponse", "Account", new { area = "User" });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }


        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorNotyf("Google ile giriş sırasında bir hata oluştu.");
                return RedirectToAction("Login", "Account", new { area = "User" });
            }

            // Google hesabıyla daha önce giriş yapılmış mı?
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                await SetUserSession(info);
                SuccessNotyf("Google ile başarıyla giriş yapıldı.");
                return RedirectToAction("Index", "Home", new { area = "User" });
            }

            // Yeni kullanıcıysa
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "Google Kullanıcısı",
                    Role = Roles.User,
                    ProfileImage = info.Principal.FindFirstValue("urn:google:picture") ?? "default-avatar.jpg"
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    ErrorNotyf("Kullanıcı oluşturulamadı: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    return RedirectToAction("Login", "Account", new { area = "User" });
                }

                await _userManager.AddLoginAsync(user, info);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            await SetUserSession(user);

            SuccessNotyf("Google hesabıyla giriş başarılı.");
            return RedirectToAction("Index", "Home", new { area = "User" });
        }

        private async Task SetUserSession(ExternalLoginInfo info)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            await SetUserSession(user);
        }

        private async Task SetUserSession(AppUser user)
        {
            var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var json = JsonConvert.SerializeObject(user, settings);
            HttpContext.Session.SetString("UserSession", json);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _accountService.FindByEmail(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Kullanıcı yoksa da aynı mesajı ver (güvenlik için)
                SuccessNotyf("Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.");
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account",
                new { area = "User", email = model.Email, token }, Request.Scheme);

            // Template replacements
            var replacements = new Dictionary<string, string>
    {
        { "{{RESET_LINK}}", callbackUrl },
        { "{{CURRENT_YEAR}}", DateTime.Now.Year.ToString() },
        { "{{EMAIL}}", model.Email }
    };

            // Email gönderimi
            try
            {
                string emailBody = await _mailService.GetEmailTemplate("PasswordResetTemplate.html", replacements);

                await _mailService.SendMail(
                    to: model.Email,
                    subject: "Şifre Sıfırlama Talebi",
                    body: emailBody
                );

                SuccessNotyf("Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.");
                return RedirectToAction("ForgotPasswordConfirmation");
            }
            catch (Exception ex)
            {
                ErrorNotyf("E-posta gönderilirken bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                ErrorNotyf("Geçersiz şifre sıfırlama bağlantısı.");
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordVM { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _accountService.FindByEmail(model.Email);
            if (user == null)
            {
                // Kullanıcı yoksa da aynı mesajı ver (güvenlik için)
                SuccessNotyf("Şifreniz başarıyla sıfırlandı.");
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                SuccessNotyf("Şifreniz başarıyla sıfırlandı.");
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}