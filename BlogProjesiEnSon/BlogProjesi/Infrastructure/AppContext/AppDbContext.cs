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

namespace Infrastructure.AppContext
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

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


        }



        public override int SaveChanges()
        {
            SetBaseProperties();
            return base.SaveChanges();
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetBaseProperties();
            return await base.SaveChangesAsync(cancellationToken);
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
