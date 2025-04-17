using System.ComponentModel.DataAnnotations;

namespace UI.Areas.User.Models.AccountVMs
{
    public class ForgotPasswordVM
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }
    }
}
