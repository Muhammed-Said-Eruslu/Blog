using Domain.Core.BaseEntity;
using Domain.Entites;

public class Comment : BaseEntity
{
    public string Content { get; set; }

    // **Post ile ilişki**
    public Guid? PostId { get; set; } // 🚀 Artık nullable olmalı
    public virtual Post Post { get; set; }

    // **Kullanıcı ile ilişki**
    public Guid? UserId { get; set; }
    public virtual AppUser User { get; set; }

    // **Oluşturulma tarihi**
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // **Yazar bilgileri (Anonim yorumlar için)**
    public string? AuthorName { get; set; }
    public string? AuthorEmail { get; set; }

    // **Parent (Üst Yorum)**
    public Guid? ParentCommentId { get; set; }
    public virtual Comment ParentComment { get; set; }

    // **Alt Yorumlar**
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

    public bool IsVisible { get; set; }
    public bool IsFlagged { get; set; }
}
