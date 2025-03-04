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
    public class PostConfiguration : IEntityTypeConfiguration<Post> 
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            // Başlık ve içerik uzunluğu kısıtlamaları
            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Content)
                .IsRequired();

            // URL'nin benzersiz olması
            builder.Property(p => p.VideoUrl)
                .IsRequired()
                .HasMaxLength(500);
                //.IsUnique(); // URL'lerin benzersiz olması

            // Excerpt'ın uzunluğu
            builder.Property(p => p.Excerpt)
                .HasMaxLength(500);

            // Post'un görseli için URL
            builder.Property(p => p.FeaturedImage)
                .HasMaxLength(500);
        }
    }
}
