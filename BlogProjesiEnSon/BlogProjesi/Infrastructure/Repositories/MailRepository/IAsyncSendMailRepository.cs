using Domain.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.MailRepository
{
    public interface IAsyncSendMailRepository
    {
        Task<string> SendConfirmMail(string to);
        Task<IResult> SendMail(string to, string subject, string body);
        Task<IResult> SendMail(string to, string subject);
        string GenerateRandomPassword(int length);
    }
}
