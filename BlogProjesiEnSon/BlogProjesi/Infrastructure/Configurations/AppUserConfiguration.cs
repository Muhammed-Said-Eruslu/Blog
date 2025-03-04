
using Domain.Entites;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations
{
    public class AppUserConfiguration :IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            // Kullanıcı adının gerekliliği ve uzunluğu
            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(256);

            // E-posta adresinin gerekliliği ve uzunluğu
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            // Parola hash'inin uzunluğu
            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(256);
            builder.Property(u => u.NormalizedEmail)
    .HasMaxLength(256);


            // Rolün varsayılan değeri
            builder.Property(u => u.Role)
                .HasDefaultValue(Roles.User);

            // Kullanıcının posts özelliği ile ilişkilendirilmesi (one-to-many)
            builder.HasMany(u => u.Posts)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
