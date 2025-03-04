using Domain.Entites;
using Domain.Enums;
using Domain.Utilities.Concretes;
using Domain.Utilities.Interfaces;
using Infrastructure.DataAccess.Interface;
using Infrastructure.Repositories.MailRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services.AccountService
{
    public class AccountService:IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMemoryCache _memoryCache;
        private readonly IAsyncSendMailRepository _asyncSendMailRepository;
        public AccountService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IMemoryCache memoryCache, IAsyncSendMailRepository asyncSendMailRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _memoryCache = memoryCache;
            _asyncSendMailRepository = asyncSendMailRepository;
        }
        public async Task<bool> AnyAsync(Expression<Func<AppUser, bool>> expression)
        {
            return await _userManager.Users.AnyAsync(expression);
        }

        public async Task<IdentityResult> CreateUserAsync(AppUser user, Roles role, string password)
        {
            user.NormalizedEmail = user.Email.ToUpper(); 
            user.NormalizedUserName = user.UserName.ToUpper(); 
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, role.ToString());
                if (roleResult.Succeeded)
                {
                    return roleResult;
                }
            }
            return result;
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            //if (_userManager == null) throw new ObjectDisposedException(nameof(_userManager)); // Servisin durumu kontrol ediliyor

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı Bulunamadı");

            return await _userManager.DeleteAsync(user);
        }

        public async Task<IdentityResult> UpdateUserAsync(AppUser user)
        {
            var existingUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (existingUser == null) throw new Exception("Kullanıcı Bulunamadı");

            existingUser.Email = user.Email;
            existingUser.UserName = user.UserName;
            existingUser.LockoutEnd = user.LockoutEnd;

            // 🛑 Güncelleme yapmadan önce normalizasyonu tekrar belirle
            existingUser.NormalizedEmail = user.Email.ToUpperInvariant();
            existingUser.NormalizedUserName = user.UserName.ToUpperInvariant();

            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded)
            {
                return result;
            }

            // 🛑 Güncelleme sonrası tekrar çek ve kontrol et
            var updatedUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (updatedUser.NormalizedEmail == null)
            {
                updatedUser.NormalizedEmail = user.Email.ToUpperInvariant();
                await _userManager.UpdateAsync(updatedUser);
            }

            return result;
        }

        public async Task<AppUser> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı Bulunamadı");
            return user;
        }

        public async Task<IResult> LoginAsync(string email, string password, bool rememberMe)
        {
            var user = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email); // ✅ NormalizedEmail yerine Email kullan

            if (user == null)
            {
                return new ErrorResult("Email veya Şifre Hatalı");
            }

            // Şifre doğrulaması
            var passwordHasher = new PasswordHasher<AppUser>();
            var verifyResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (verifyResult == PasswordVerificationResult.Success)
            {
                // Şifre doğruysa giriş yap
                await _signInManager.SignInAsync(user, isPersistent: rememberMe);
                return new SuccessResult("Giriş Başarılı");
            }

            return new ErrorResult("Email veya Şifre Hatalı");
        }





        public async Task<IResult> LogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return new SuccessResult("Çıkış Yapıldı");
            }
            catch (Exception)
            {

                return new ErrorResult("Çıkış Yapılamadı");
            }

        }
        public async Task<IResult> PasswordChangeAsync(AppUser appUser, string password)
        {
            if (appUser != null && password != null)
            {
                try
                {
                    appUser.PasswordHash = _userManager.PasswordHasher.HashPassword(appUser, password);
                    var result = await _userManager.UpdateAsync(appUser);
                    if (result.Succeeded)
                    {
                        return new SuccessResult("Şifreniz Değiştirildi");
                    }
                    return new ErrorResult("Şifre Değiştirilemedi");
                }
                catch (Exception ex)
                {
                    return new ErrorResult("Şifre Değiştirilemedi :" + ex.Message);
                }
            }
            else
                return new ErrorResult("Şifre Değiştirilemedi");
        }

        public async Task<AppUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public string HashPassword(AppUser user, string password)
        {
            // Örneğin, kullanıcı ID'sini salt olarak ekleyebilirsiniz:
            string salt = user.Id.ToString();
            string combined = password + salt;
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToBase64String(hashedBytes);
            }
        }
        private string GenerateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8); // 8 karakterli random şifre
        }

        public bool VerifyTemporaryPassword(AppUser user, string hashedPassword, string providedPassword)
        {
            var passwordHasher = new PasswordHasher<AppUser>();
            var result = passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }
        public bool VerifyTwoFactorCode(AppUser user, string providedCode)
        {
            var cacheKey = $"TwoFactorCode_{user.Email.ToLower().Trim()}";
            if (_memoryCache.TryGetValue(cacheKey, out string storedCode))
            {
                return storedCode == providedCode;
            }
            return false;
        }

        public async Task<string> SetTwoFactorCodeAsync(AppUser user)
        {
            // 100000 ile 999999 arasında rastgele 6 haneli kod oluştur
            var code = new Random().Next(100000, 999999).ToString();

            // Cache anahtarınızı tutarlı bir biçimde oluşturun
            var cacheKey = $"TwoFactorCode_{user.Email.ToLower().Trim()}";

            // Kodu cache'e, 5 dakika geçerli olacak şekilde ekleyin
            _memoryCache.Set(cacheKey, code, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            // Örneğin, bu kodu e-posta veya SMS ile kullanıcıya göndereceksiniz.
            // Bu metot doğrudan kodu geri döndürüyor.
            return code;
        }

        public async Task<string> SetTemporaryPasswordAsync(AppUser user)
        {
            var code = new Random().Next(100000, 999999).ToString();

            //user.TemporaryPassword = HashPassword(user, code);
            await UpdateUserAsync(user);

            return code;
        }

        public bool VerifyPassword(AppUser user, string providedPassword, string hashedPassword)
        {
            var passwordHasher = new PasswordHasher<AppUser>();
            var result = passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<AppUser> FindByEmail(string mail)
        {
            var user = await _userManager.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Email == mail); // Sadece Email ile ara

            return user;
        }






        public async Task<string> SendConfirmMail(string to)
        {
            try
            {
                return await _asyncSendMailRepository.SendConfirmMail(to);
            }
            catch (Exception ex)
            {
                return "Dogrulama Kodu Gönderilemedi";
            }
        }

        public async Task<IResult> SendMail(string to, string subject, string body)
        {
            try
            {
                return await _asyncSendMailRepository.SendMail(to, subject, body);
            }
            catch (Exception ex)
            {
                return new ErrorResult("Dogrulama Kodu Gönderilemedi");
            }
        }

        public async Task<IResult> SendMail(string to, string subject)
        {
            try
            {
                return await _asyncSendMailRepository.SendMail(to, subject);
            }
            catch (Exception ex)
            {
                return new ErrorResult("Dogrulama Kodu Gönderilemedi");
            }
        }

     
    }
}
