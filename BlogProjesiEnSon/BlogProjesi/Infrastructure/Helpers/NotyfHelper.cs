using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Helpers
{
    public static class NotyfHelper
    {
        private static INotyfService GetNotyf()
        {
            var httpContextAccessor = new HttpContextAccessor();
            var httpContext = httpContextAccessor.HttpContext;

            return httpContext?.RequestServices.GetService<INotyfService>();
        }

        public static void SuccessNotyf(string message) => GetNotyf()?.Success(message);
        public static void ErrorNotyf(string message) => GetNotyf()?.Error(message);
        public static void WarningNotyf(string message) => GetNotyf()?.Warning(message);
        public static void InfoNotyf(string message) => GetNotyf()?.Information(message);
    }
}
