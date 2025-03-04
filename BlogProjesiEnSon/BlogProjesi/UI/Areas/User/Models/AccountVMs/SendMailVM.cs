namespace UI.Areas.User.Models.AccountVMs
{
    public class SendMailVM
    {
        public string? UserCode { get; set; }
        public string? ConfirmCode { get; set; }
        public RegisterVM RegisterVM { get; set; }
    }
}
