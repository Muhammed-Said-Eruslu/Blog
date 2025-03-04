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
    public class CommentConfiguration : IEntityTypeConfiguration<Comment> 
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            // Comment Id'si otomatik olarak BaseEntity'den miras alır ve Guid tipinde olacaktır.

            // Content alanı zorunlu
            builder.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(500); // İçerik uzunluğu sınırı ekledik (opsiyonel, gereksinime göre ayarlayabilirsiniz)

            // Post ile olan ilişkiyi ayarlıyoruz
            builder.HasOne(c => c.Post)
                .WithMany(p => p.Comments) // Bir post birden fazla yorum alabilir
                .HasForeignKey(c => c.PostId) // Yorumda hangi post'a ait olduğunu belirtiyoruz
                .OnDelete(DeleteBehavior.Cascade); // Post silindiğinde, ilişkili yorumlar da silinsin

            // User ile olan ilişkiyi ayarlıyoruz (opsiyonel, kullanıcı zorunlu değil)
            builder.HasOne(c => c.User)
                .WithMany() // Kullanıcı birden fazla yorum yazabilir
                .HasForeignKey(c => c.UserId) // Yorumun hangi kullanıcıya ait olduğunu belirtiyoruz
                .OnDelete(DeleteBehavior.SetNull); // Kullanıcı silindiğinde, ilişkili yorumun kullanıcı bilgisini null yap

            // ParentComment ile olan ilişkiyi ayarlıyoruz (Reply yapısına yönelik)
            builder.HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies) // Bir yorumun birden fazla yanıtı (reply) olabilir
                .HasForeignKey(c => c.ParentCommentId) // Yorumun hangi yoruma ait olduğunu belirtiyoruz
                .OnDelete(DeleteBehavior.Cascade); // ParentComment silindiğinde, yanıtlar (replies) da silinsin

            // AuthorName ve AuthorEmail opsiyonel olduğu için null yapılabilir
            builder.Property(c => c.AuthorName)
                .HasMaxLength(100)
                .IsRequired(false); // Optional alan, null yapılabilir

            builder.Property(c => c.AuthorEmail)
                .HasMaxLength(100)
                .IsRequired(false); // Optional alan, null yapılabilir

            // ParentCommentId, nullable olduğu için opsiyonel alan
            builder.Property(c => c.ParentCommentId)
                .IsRequired(false); // ParentCommentId opsiyonel

            // Comment tablosunda Index oluşturmak, performansı artırabilir (isteğe bağlı)
            builder.HasIndex(c => c.PostId);
        }
    }
}
