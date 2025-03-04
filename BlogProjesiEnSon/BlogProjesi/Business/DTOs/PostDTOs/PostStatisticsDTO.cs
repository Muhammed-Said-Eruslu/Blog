using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DTOs.PostDTOs
{
    public class PostStatisticsDTO
    {
        // Toplam post sayısı
        public int TotalPosts { get; set; }

        // Kategorilere göre post sayısı
        public List<CategoryPostCountDTO> PostCountByCategory { get; set; } = new List<CategoryPostCountDTO>();

        // En popüler post'lar (örneğin, en çok görüntülenen)
        public List<PostListDTO> PopularPosts { get; set; } = new List<PostListDTO>();

        // En çok yorum alan post'lar
        public List<PostListDTO> MostCommentedPosts { get; set; } = new List<PostListDTO>();

        // Diğer istatistikler
        public int TotalComments { get; set; }
        public int TotalTags { get; set; }
    }

    public class CategoryPostCountDTO
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int PostCount { get; set; }
    }
}

