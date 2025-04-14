using AspNetCoreHero.ToastNotification.Abstractions;
using Business.Services.SubscriberService;
using Infrastructure.Repositories.MailRepository;
using Microsoft.AspNetCore.Mvc;

namespace UI.Areas.User.Controllers
{
    [Area("User")]
    public class CommonController : Controller
    {
        private readonly ISubscriberService _subscriberService;
        private readonly INotyfService _notyf;
        private readonly IAsyncSendMailRepository _mailService;
        public CommonController(ISubscriberService subscriberService, INotyfService notyf, IAsyncSendMailRepository mailService)
        {
            _subscriberService = subscriberService;
            _notyf = notyf;
            _mailService = mailService;
        }
        [HttpPost]
        public async Task<IActionResult> Subscribe(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                TempData["SubscribeStatus"] = "error";
                TempData["SubscribeMessage"] = "Lütfen geçerli bir e-posta adresi girin.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            bool result = await _subscriberService.AddAsync(email);

            if (result)
            {
                TempData["SubscribeStatus"] = "success";
                TempData["SubscribeMessage"] = "Abonelik başarılı! Teşekkür ederiz.";

                
                string templateName = "Subs.html";
                var replacements = new Dictionary<string, string>
        {
            { "{{EMAIL}}", email }
        };

                string emailBody = await _mailService.GetEmailTemplate(templateName, replacements);

                await _mailService.SendMail(
                    to: email,
                    subject: "Bülten Aboneliğiniz Başarılı",
                    body: emailBody
                );
            }
            else
            {
                TempData["SubscribeStatus"] = "warning";
                TempData["SubscribeMessage"] = "Zaten abonesiniz.";
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }



    }
}
