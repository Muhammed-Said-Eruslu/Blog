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
    public class TagConfiguration : IEntityTypeConfiguration<Tag> 
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            // Name ve Slug alanlarını zorunlu yapıyoruz
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100); // Tag isminin uzunluğunu sınırlıyoruz

            builder.Property(t => t.Slug)
                .IsRequired()
                .HasMaxLength(100); // Slug alanını da sınırlıyoruz

            // Name ve Slug'un benzersiz olmasını sağlıyoruz
            builder.HasIndex(t => t.Name)
                .IsUnique(); // Tag isimlerinin benzersiz olması gerektiği için indeks ekliyoruz

            builder.HasIndex(t => t.Slug)
                .IsUnique(); // Slug alanı da benzersiz olmalı

            // Tag ile ilgili PostTag ilişkisini ayarlıyoruz
            builder.HasMany(t => t.PostTags) // Bir Tag birden fazla PostTag ile ilişkili olabilir
                .WithOne(pt => pt.Tag) // PostTag'in bir Tag'i olmalı
                .HasForeignKey(pt => pt.TagId) // Yabancı anahtar TagId
                .OnDelete(DeleteBehavior.Cascade); // Tag silindiğinde ilişkili PostTag silinsin
        }
    }
}
