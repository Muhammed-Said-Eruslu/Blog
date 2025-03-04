using Domain.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category> 
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            // Kategorinin isminin gerekliliği ve uzunluğu
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);  // Kategori ismi için max uzunluk belirliyoruz

            // Slug alanının gerekliliği ve uzunluğu
            builder.Property(c => c.Slug)
                .IsRequired()
                .HasMaxLength(100);  // Slug için max uzunluk

            // Slug'un benzersiz olması (kategori slug'larının tekrarlanmaması için)
            builder.HasIndex(c => c.Slug)
                .IsUnique();

            // Açıklama alanı isteğe bağlı (null olabilir)
            builder.Property(c => c.Description)
                .HasMaxLength(500);  // Açıklama uzunluğunu sınırlıyoruz

            // Category ve Post arasında ilişki (one-to-many)
            builder.HasMany(c => c.Posts)
                .WithOne(p => p.Category)  // Kategori her post için bir tane olacak
                .HasForeignKey(p => p.CategoryId)  // Post'ta CategoryId foreign key olacak
                .OnDelete(DeleteBehavior.Cascade);  // Kategori silindiğinde ona ait postlar da silinsin
        }
    }
}
