using Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services.SubscriberService
{
    public interface ISubscriberService
    {
        Task<bool> AddAsync(string email);
        Task<bool> IsSubscribed(string email);
        Task<List<Subscriber>> GetAllSubscribersAsync();
    }
}
