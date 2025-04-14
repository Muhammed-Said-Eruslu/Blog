namespace UI.Areas.Admin.Models.VMs.DashboardVM
{
    public class DashboardViewModel
    {
        public int TotalPosts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalComments { get; set; }
        public int TotalUsers { get; set; }

        // Ek göstergeler istersen (popüler yazılar, son yazılar, vb.)
        public List<string> PopularPosts { get; set; }
        public List<string> RecentPosts { get; set; }

        // İstersen son 24 saatte eklenen yazıların sayısı gibi alanlar da ekleyebilirsin:
        public int NewPostsLast24Hours { get; set; }
        public int NewCommentsLast24Hours { get; set; }
        public int[] MonthlyPostCounts { get; set; }

    }
}
