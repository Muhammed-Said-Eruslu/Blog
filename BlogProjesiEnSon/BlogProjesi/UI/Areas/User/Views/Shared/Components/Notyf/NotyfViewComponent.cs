using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
namespace UI.Areas.User.Views.Shared.Components.Notyf
{
    public class NotyfViewComponent : ViewComponent
    {
        private readonly INotyfService _notyf;

        public NotyfViewComponent(INotyfService notyf)
        {
            _notyf = notyf;
        }

        public IViewComponentResult Invoke()
        {
            return View(_notyf);
        }
    }
}
