

using Domain.Enums;

namespace UI.Areas.User.Models.AccountVMs
{
    public class RegisterVM
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? PhotoUrl { get; set; }
        public Gender Gender { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public DateTime? BirthDate { get; set; }
    }
}
