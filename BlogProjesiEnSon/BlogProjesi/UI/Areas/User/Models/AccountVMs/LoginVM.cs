using System.ComponentModel.DataAnnotations;

namespace UI.Areas.User.Models.AccountVMs
{
    public class LoginVM
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Şifre zorunludur")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
