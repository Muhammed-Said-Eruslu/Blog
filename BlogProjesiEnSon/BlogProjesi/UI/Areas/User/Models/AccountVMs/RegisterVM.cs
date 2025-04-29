

using Business.Validation;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace UI.Areas.User.Models.AccountVMs
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        public string? PhotoUrl { get; set; }

        [Required(ErrorMessage = "Cinsiyet seçimi zorunludur.")]
        [Display(Name = "Cinsiyet")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{8,}$",
            ErrorMessage = "Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.")]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        [AgeValidation(MinimumAge = 13, LatestAllowedYear = 2024, ErrorMessage = "13 yaşından büyük ve geçerli bir doğum tarihi giriniz.")]
        public DateTime? BirthDate { get; set; }
    }
}
