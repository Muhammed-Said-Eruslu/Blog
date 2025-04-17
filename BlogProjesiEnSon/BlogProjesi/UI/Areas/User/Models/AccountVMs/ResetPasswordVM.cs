using System.ComponentModel.DataAnnotations;

namespace UI.Areas.User.Models.AccountVMs
{
    public class ResetPasswordVM
    {
        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        [Required(ErrorMessage = "Şifreyi tekrar girmeniz gerekmektedir.")]
        public string ConfirmPassword { get; set; }
    }
}
