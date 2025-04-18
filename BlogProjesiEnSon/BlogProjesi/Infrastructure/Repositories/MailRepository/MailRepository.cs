using Domain.Utilities.Concretes;
using Domain.Utilities.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Infrastructure.Repositories.MailRepository
{
    public class MailRepository : IAsyncSendMailRepository
    {
        // API key'ler ve SMTP bilgileri burada sabit olarak duruyor.
        private const string _smtpServer = "in-v3.mailjet.com";
        private const int _smtpPort = 587;
        private const string _apiKeyPublic = "f21080bb92ec1a9bdcdd0e720abd9089";
        private const string _apiKeyPrivate = "886e64b947f15e34496fd9d6bf974a59";
        private const string _senderEmail = "saideruslu0@gmail.com";
        private const string _senderName = "Blog Projesi";

        public string GenerateRandomPassword(int length = 12)
        {
            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialChars = "!@#$%^&*()";

            var random = new Random();
            var password = new StringBuilder();

            // Her kategoriden en az bir karakter ekleyelim
            password.Append(upperCase[random.Next(upperCase.Length)]);
            password.Append(lowerCase[random.Next(lowerCase.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            password.Append(specialChars[random.Next(specialChars.Length)]);

            // Kalan karakterleri rastgele ekleyelim
            for (var i = 4; i < length; i++)
            {
                var allChars = upperCase + lowerCase + digits + specialChars;
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Karakterleri karıştırarak döndür
            return new string(password.ToString().OrderBy(c => random.Next()).ToArray());
        }

        public async Task<string> SendConfirmMail(string to)
        {
            try
            {
                // ✅ Doğrulama kodu oluştur
                var random = new Random();
                string kod = random.Next(10000, 99999).ToString();

                // ✅ E-posta şablonunu al
                string emailBody = await GetEmailTemplate("EmailTemplate.html", new Dictionary<string, string>
                {
                    { "{{CODE}}", kod }
                });

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_senderName, _senderEmail));
                message.To.Add(new MailboxAddress("Kullanıcı", to));
                message.Subject = "E-Posta Doğrulama Kodu";
                message.Body = new TextPart(TextFormat.Html) { Text = emailBody };

                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_apiKeyPublic, _apiKeyPrivate);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return kod;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                return "0";
            }
        }

        public async Task<IResult> SendMail(string to, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_senderName, _senderEmail));
                message.To.Add(new MailboxAddress("Kullanıcı", to));
                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html) { Text = body };

                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_apiKeyPublic, _apiKeyPrivate);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return new SuccessResult("E-posta başarıyla gönderildi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"E-posta gönderilemedi: {ex.Message}");
            }
        }

        public async Task<IResult> SendMail(string to, string subject)
        {
            return await SendMail(to, subject, "E-postanız hazırlanıyor...");
        }
        public async Task<string> GetEmailTemplate(string templateName, Dictionary<string, string> replacements)
        {
            // wwwroot/templates klasörüne göre yol oluştur
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", templateName);

            if (!File.Exists(templatePath))
            {
                return "<html><body><h2>Şablon bulunamadı</h2></body></html>";
            }

            string emailBody = await File.ReadAllTextAsync(templatePath);

            foreach (var replacement in replacements)
            {
                emailBody = emailBody.Replace(replacement.Key, replacement.Value);
            }

            return emailBody;
        }

        public async Task<bool> SendConfirmMail(string to, string confirmCode)
        {
            try
            {
                // E-posta şablonunu al
                string emailBody = await GetEmailTemplate("ConfirmEmailTemplate.html", new Dictionary<string, string>
        {
            { "{{CONFIRM_CODE}}", confirmCode },
            { "{{EMAIL}}", to },
            { "{{CURRENT_YEAR}}", DateTime.Now.Year.ToString() }
        });

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_senderName, _senderEmail));
                message.To.Add(new MailboxAddress("Kullanıcı", to));
                message.Subject = "E-Posta Doğrulama Kodu";
                message.Body = new TextPart(TextFormat.Html) { Text = emailBody };

                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_apiKeyPublic, _apiKeyPrivate);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                return false;
            }
        }

    }
}
