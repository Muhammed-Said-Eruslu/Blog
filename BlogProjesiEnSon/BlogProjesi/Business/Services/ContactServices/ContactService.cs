using Domain.Entites;
using Domain.Utilities.Concretes;
using Domain.Utilities.Interfaces;
using Infrastructure.Repositories.ContactRepository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.ContactServices
{
    public class ContactService : IContactService
    {
        private readonly IContactRepistory _contactRepistory;

        public ContactService(IContactRepistory contactRepistory)
        {
            _contactRepistory = contactRepistory;
        }

        public async Task<IResult> AddAsync(Contact contact)
        {
            if (contact == null)
            {
                return new ErrorResult("İletişim bilgileri boş olduğu için eklenemedi.");
            }
            await _contactRepistory.AddAsync(contact);
            _contactRepistory.SaveChangeAsync();
            return new SuccessResult("İletişim ekleme başarılı.");
        }

        public async Task<IResult> DeleteAsync(Guid id)
        {
            var contact = await _contactRepistory.GetByIdAsync(id);
            if (contact == null)
            {
                return new ErrorResult("İletişim bulunamadı.");
            }
            await _contactRepistory.DeleteAsync(contact);
            await _contactRepistory.SaveChangeAsync();
            return new SuccessResult("İletişim bilgileri başarılı bir şekilde silindi.");
        }

        public async Task<IDataResult<List<Contact>>> GetAllAsync()
        {
            var contacts = await _contactRepistory.GetAllAsync();
            return new SuccessDataResult<List<Contact>>(contacts.ToList(), "İletişim listesi başarılı bir şekilde getirildi.");
        }

        public async Task<IDataResult<Contact>> GetByIdAsync(Guid id)
        {
            var contact = await _contactRepistory.GetByIdAsync(id);
            if (contact == null)
            {
                return new ErrorDataResult<Contact>("İletişim bulunamadı.");
            }
            return new SuccessDataResult<Contact>(contact, "İletişim başarılı bir şekilde getirildi.");
        }

        public async Task<IResult> UpdateAsync(Contact contact)
        {
            if (contact == null)
            {
                return new ErrorResult("Güncellenecek iletişim bilgileri bulunamadı.");
            }

            var existingContact = await _contactRepistory.GetByIdAsync(contact.Id);
            if (existingContact == null)
            {
                return new ErrorResult("İletişim bulunamadı.");
            }

            await _contactRepistory.UpdateAsync(contact);
            return new SuccessResult("İletişim güncelleme başarılı.");
        }
    }
}