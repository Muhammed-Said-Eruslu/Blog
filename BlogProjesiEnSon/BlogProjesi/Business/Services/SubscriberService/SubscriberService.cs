using Domain.Entites;
using Infrastructure.Repositories.SubscriberRepository;
using System.Threading.Tasks;

namespace Business.Services.SubscriberService
{
    public class SubscriberService : ISubscriberService
    {
        private readonly ISubscriberRepository _subscriberRepository;

        public SubscriberService(ISubscriberRepository subscriberRepository)
        {
            _subscriberRepository = subscriberRepository;
        }

        public async Task<bool> AddAsync(string email)
        {
            var isAlreadySubscribed = await _subscriberRepository.AnyAsync(s => s.Email == email);
            if (isAlreadySubscribed)
                return false;

            var newSubscriber = new Subscriber
            {
                Email = email
            };

            await _subscriberRepository.AddAsync(newSubscriber);
            await _subscriberRepository.SaveChangeAsync();

            return true;
        }

        public async Task<bool> IsSubscribed(string email)
        {
            return await _subscriberRepository.AnyAsync(s => s.Email == email);
        }
    }
}
