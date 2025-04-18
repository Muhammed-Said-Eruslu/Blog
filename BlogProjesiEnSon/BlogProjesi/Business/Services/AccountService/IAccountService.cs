using Castle.Core.Resource;
using Domain.Entites;
using Domain.Enums;
using Domain.Utilities.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services.AccountService
{
    public interface IAccountService
    {
        Task<bool> AnyAsync(Expression<Func<AppUser, bool>> expression);
        Task<IdentityResult> CreateUserAsync(AppUser user, Roles role, string password);
        Task<IdentityResult> DeleteUserAsync(string userId);
        Task<IdentityResult> UpdateUserAsync(AppUser user);
        Task<AppUser> GetUserByIdAsync(string userId);
        Task<IResult> LoginAsync(string email, string password, bool rememberMe);
        Task<IResult> LogoutAsync();
        Task<AppUser> FindByEmail(string mail);
        Task<IResult> PasswordChangeAsync(AppUser appUser, string password);
        Task<AppUser> GetUserByEmailAsync(string email);
        string HashPassword(string password);
        string HashPassword(AppUser user, string password);
        bool VerifyTemporaryPassword(AppUser user, string hashedPassword, string providedPassword);
        public bool VerifyTwoFactorCode(AppUser user, string providedCode);

        Task<string> SetTemporaryPasswordAsync(AppUser user);
        Task<string> SetTwoFactorCodeAsync(AppUser user);
        bool VerifyPassword(AppUser user, string providedPassword, string hashedPassword);
        Task<string> SendConfirmMail(string to);
        Task<IResult> SendMail(string to, string subject, string body);
        Task<IResult> SendMail(string to, string subject);
        Task<string> GeneratePasswordResetTokenAsync(AppUser user);
        Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string newPassword);
        Task<bool> SendPasswordResetEmail(string email, string resetLink);
        Task<string> SendConfirmMail(string email, string confirmCode);
    }
}
