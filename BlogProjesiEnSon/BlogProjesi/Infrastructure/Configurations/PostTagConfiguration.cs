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
    public class PostTagConfiguration : IEntityTypeConfiguration<PostTag> 
    {
        public void Configure(EntityTypeBuilder<PostTag> builder)
        {
            // Bir PostTag, bir Post ve bir Tag ile ilişkilidir
            // Post ve Tag arasındaki ilişkiyi yapıyoruz

            builder.HasKey(pt => new { pt.PostId, pt.TagId }); // PostId ve TagId birincil anahtar olarak kullanılır

            // Post ile olan ilişkiyi ayarlıyoruz
            builder.HasOne(pt => pt.Post)
                .WithMany(p => p.PostTag) // Bir Post birden fazla Tag ile ilişkili olabilir
                .HasForeignKey(pt => pt.PostId) // Yabancı anahtar PostId
                .OnDelete(DeleteBehavior.Cascade); // Post silindiğinde ilişkili PostTag silinsin

            // Tag ile olan ilişkiyi ayarlıyoruz
            builder.HasOne(pt => pt.Tag)
                .WithMany(t => t.PostTags) // Bir Tag birden fazla Post ile ilişkili olabilir
                .HasForeignKey(pt => pt.TagId) // Yabancı anahtar TagId
                .OnDelete(DeleteBehavior.Cascade); // Tag silindiğinde ilişkili PostTag silinsin
        }
    }
}
