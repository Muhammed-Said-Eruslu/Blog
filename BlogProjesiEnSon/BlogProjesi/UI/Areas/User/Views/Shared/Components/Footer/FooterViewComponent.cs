using Infrastructure.AppContext;
using Microsoft.AspNetCore.Mvc;
using UI.Areas.User.Models.FooterVMs;
namespace UI.Areas.User.Views.Shared.Components.Footer
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        public FooterViewComponent(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // En popüler 3 kategoriyi çekme (post sayısına göre)
            var categories = _context.Categories
                .OrderByDescending(c => c.Posts.Count)
                .Take(3)
                .Select(c => new CategoryViewModel
                {
                    Name = c.Name,
                    PostCount = c.Posts.Count,
                    Url = $"/Category/{c.Slug}"
                })
                .ToList();

            // Son 3 ayın arşivini çekme (içerik olmasa bile)
            var archives = new List<ArchiveViewModel>();
            var currentDate = DateTime.Now;

            // Son 3 ayı her durumda listeye ekle
            for (int i = 0; i < 3; i++)
            {
                var targetMonth = currentDate.AddMonths(-i);

                // Ay adını Türkçe'ye çevir
                string monthName = GetTurkishMonthName(targetMonth.Month);

                archives.Add(new ArchiveViewModel
                {
                    MonthYear = $"{monthName} {targetMonth.Year}",
                    PostCount = 0, // Post sayısını göstermeyeceğiz
                    Url = $"/Archive/{targetMonth.Year}/{targetMonth.Month}"
                });
            }

            var viewModel = new FooterViewModel
            {
                Categories = categories,
                Archives = archives
            };

            return View(viewModel);
        }

        // Ay isimlerini Türkçe olarak döndüren yardımcı metot
        private string GetTurkishMonthName(int month)
        {
            switch (month)
            {
                case 1: return "Ocak";
                case 2: return "Şubat";
                case 3: return "Mart";
                case 4: return "Nisan";
                case 5: return "Mayıs";
                case 6: return "Haziran";
                case 7: return "Temmuz";
                case 8: return "Ağustos";
                case 9: return "Eylül";
                case 10: return "Ekim";
                case 11: return "Kasım";
                case 12: return "Aralık";
                default: return "";
            }
        }
    }
}