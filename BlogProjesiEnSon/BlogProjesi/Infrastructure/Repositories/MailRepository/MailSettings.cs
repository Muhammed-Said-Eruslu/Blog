using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.MailRepository
{
    public class MailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string ApiKeyPublic { get; set; }
        public string ApiKeyPrivate { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
    }
}
