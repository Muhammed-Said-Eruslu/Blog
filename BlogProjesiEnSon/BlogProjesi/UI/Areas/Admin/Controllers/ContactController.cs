using Business.Services.ContactServices;
using Domain.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ContactController : Controller
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _contactService.GetAllAsync();
            if (!result.IsSucces)
            {
                // Handle error, maybe show a message
                return View(new List<Contact>());
            }
            return View(result.Data);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _contactService.GetByIdAsync(id);
            if (!result.IsSucces)
            {
                return NotFound();
            }
            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _contactService.DeleteAsync(id);
            if (!result.IsSucces)
            {
                // Handle error
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
