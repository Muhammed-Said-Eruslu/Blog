using Business.DTOs.CategoryDTOs;
using Domain.Entites;
using Domain.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services.ContactServices
{
    public interface IContactService
    {
        Task<IResult> AddAsync(Contact contact);
        Task<IDataResult<List<Contact>>> GetAllAsync();
        Task<IResult> DeleteAsync(Guid id);
        Task<IDataResult<Contact>> GetByIdAsync(Guid id);
        Task<IResult> UpdateAsync(Contact contact);
    }
}
