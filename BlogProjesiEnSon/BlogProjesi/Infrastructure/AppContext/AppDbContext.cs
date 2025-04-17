using Domain.Core.BaseEntity;
using Domain.Entites;
using Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Infrastructure.AppContext
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        private readonly ILogger<AppDbContext> _logger; // Logger tanımlaması

        public AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger) : base(options)
        {
            _logger = logger; // Logger'ı constructor üzerinden inject ediyoruz
        }

        public virtual DbSet<AppUser> Users { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<PostTag> PostTags { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<AppUserPost> AppUserPosts { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<Subscriber> Subscribers { get; set; }
        public virtual DbSet<Photo> Photos { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Many-to-Many ilişkisini ara tablo ile tanımla
            builder.Entity<AppUserPost>()
                .HasKey(up => new { up.UserId, up.PostId }); // Composite Key

            builder.Entity<AppUserPost>()
                .HasOne(up => up.User)
                .WithMany(u => u.LikedPosts)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete kullanıyoruz

            builder.Entity<AppUserPost>()
                .HasOne(up => up.Post)
                .WithMany(p => p.LikedUsers)
                .HasForeignKey(up => up.PostId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete kullanıyoruz

            // **Yorum - Parent Yorum İlişkisi**
            builder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction); // ❌ Çakışmayı önlemek için NoAction

            // **Yorum - Post İlişkisi** 
            builder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.NoAction); // ❌ FK Çakışmasını önlemek için NoAction

            // **Yorum - Kullanıcı İlişkisi**  
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull); // ✅ Kullanıcı silinirse yorumun UserId NULL olur.

            builder.Entity<Post>(entity =>
            {
                entity.Property(p => p.RowVersion)
                      .IsRowVersion()
                      .IsConcurrencyToken();
            });

            builder.Entity<PostTag>()
    .HasKey(pt => new { pt.PostId, pt.TagId });

            builder.Entity<PostTag>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.PostTag)
                .HasForeignKey(pt => pt.PostId);

            builder.Entity<PostTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.TagId);

        }



        public override int SaveChanges()
        {
            SetBaseProperties();
            return base.SaveChanges();
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            const int maxRetryCount = 99;
            int retryCount = 0;

            while (true)
            {
                try
                {
                    SetBaseProperties();
                    return await base.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException ex) when (retryCount < maxRetryCount)
                {
                    retryCount++;
                    foreach (var entry in ex.Entries)
                    {
                        var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

                        if (databaseValues == null)
                        {
                            entry.State = EntityState.Detached;
                            continue;
                        }

                        // Client wins stratejisi
                        entry.OriginalValues.SetValues(databaseValues);
                    }

                    // 100ms bekleyip tekrar dene
                    await Task.Delay(100, cancellationToken);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Veritabanı güncelleme hatası");
                    throw new ApplicationException("Veritabanı işlemi sırasında bir hata oluştu. Lütfen tekrar deneyin.", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Beklenmeyen veritabanı hatası");
                    throw new ApplicationException("Sistem hatası oluştu. Lütfen daha sonra tekrar deneyin.", ex);
                }
            }
        }
        private void SetBaseProperties()
        {
            var entries = ChangeTracker.Entries<BaseEntity>(); // Bu sınıf context tarafından db ye gönderilen işlem bloklarını collection olarak barındırır. Her işlem bloğunun işlem tipi (State) ve entityleri de içerisinde bulunur.

            var userId = "UserBulunamadı";

            foreach (var entry in entries)
            {
                SetIfAdded(entry, userId);
                SetIfModifed(entry, userId);
                if (entry.Entity is AuditableEntity)
                {
                    SetIfDeleted(entry, userId);
                }
            }
        }
        private void SetIfDeleted(EntityEntry<BaseEntity> entry, string userId)
        {
            if (entry.State != EntityState.Deleted)
                return;
            if (entry.Entity is not AuditableEntity entity)
                return;
            entry.State = EntityState.Modified;
            entry.Entity.Status = Status.Deleted;
            entity.DeletedDate = DateTime.Now;
            entity.DeletedBy = userId;

        }
        private void SetIfModifed(EntityEntry<BaseEntity> entry, string userId)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Status = Status.Modified;
                entry.Entity.UpdatedBy = userId;
                entry.Entity.UpdatedDate = DateTime.Now;
            }
        }
        private void SetIfAdded(EntityEntry<BaseEntity> entry, string userId)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Status = Status.Added;
                entry.Entity.CreatedBy = userId;
                entry.Entity.CreatedDate = DateTime.Now;
            }
        }
    }
}
