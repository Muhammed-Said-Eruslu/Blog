using AspNetCoreHero.ToastNotification.Abstractions;
using Business.Services.SubscriberService;
using Infrastructure.Repositories.MailRepository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

                try
                {
                    string templateName = "Subs.html"; // Template dosyasının adı
                    var replacements = new Dictionary<string, string>
                    {
                        { "{{EMAIL}}", email },
                        { "{{CURRENT_YEAR}}", DateTime.Now.Year.ToString() }
                    };

                    // Email içeriği template ile oluşturuluyor
                    string emailBody = await _mailService.GetEmailTemplate(templateName, replacements);

                    // Mail gönderimi yapılıyor
                    await _mailService.SendMail(
                        to: email,
                        subject: "Bülten Aboneliğiniz Başarılı",
                        body: emailBody
                    );
                }
                catch (Exception ex)
                {
                    TempData["SubscribeStatus"] = "error";
                    TempData["SubscribeMessage"] = $"Mail gönderimi sırasında bir hata oluştu: {ex.Message}";
                }
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
