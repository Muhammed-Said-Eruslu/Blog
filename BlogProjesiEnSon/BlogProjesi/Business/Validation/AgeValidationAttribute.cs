using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Validation
{
    public class AgeValidationAttribute : ValidationAttribute
    {
        public int MinimumAge { get; set; }
        public int LatestAllowedYear { get; set; } = DateTime.Now.Year; // varsayılan olarak bu yıl

        public override bool IsValid(object value)
        {
            if (value == null) return false;

            var date = (DateTime)value;

            // Gelecek tarih engeli
            if (date.Year > LatestAllowedYear) return false;

            // Yaş hesapla
            var age = DateTime.Now.Year - date.Year;
            if (date > DateTime.Now.AddYears(-age)) age--;

            return age >= MinimumAge;
        }
    }
}
