using Bogus;
using Domain.Entites;
using Domain.Enums;
using Infrastructure.AppContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Infrastructure.Seeds
{
    public class FakeDataGenerator
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            try
            {
                Console.WriteLine("🔄 Fake Data Oluşturuluyor...");

                // 1. Kullanıcılar (Users)
                if (!await context.Users.AnyAsync())
                {
                    var users = GenerateUsers(10);
                    await context.Users.AddRangeAsync(users);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Kullanıcılar eklendi.");
                }

                // 2. Kategoriler (Categories)
                if (!await context.Categories.AnyAsync())
                {
                    var categories = GenerateCategories(5);
                    await context.Categories.AddRangeAsync(categories);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Kategoriler eklendi.");
                }

                // 3. Etiketler (Tags)
                if (!await context.Tags.AnyAsync())
                {
                    var tags = GenerateTags(15);
                    await context.Tags.AddRangeAsync(tags);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Etiketler eklendi.");
                }

                // 4. Kullanıcılar ve Kategoriler yeniden çekiliyor
                var usersFromDb = await context.Users.ToListAsync();
                var categoriesFromDb = await context.Categories.ToListAsync();

                // Eğer listeler boşsa varsayılan ekle (bu adım genelde gerekmiyor ama kontrol için)
                if (!usersFromDb.Any())
                {
                    var defaultUser = new AppUser
                    {
                        Id = Guid.NewGuid(),
                        UserName = "admin",
                        Email = "admin@example.com",
                        PasswordHash = "Admin123!",
                        FullName = "Admin User",
                        Role = Roles.Admin
                    };
                    await context.Users.AddAsync(defaultUser);
                    await context.SaveChangesAsync();
                    usersFromDb.Add(defaultUser);
                    Console.WriteLine("⚠️ Varsayılan kullanıcı eklendi.");
                }
                if (!categoriesFromDb.Any())
                {
                    var defaultCategory = new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = "Genel",
                        Slug = "genel",
                        Description = "Genel kategori açıklaması"
                    };
                    await context.Categories.AddAsync(defaultCategory);
                    await context.SaveChangesAsync();
                    categoriesFromDb.Add(defaultCategory);
                    Console.WriteLine("⚠️ Varsayılan kategori eklendi.");
                }

                // 5. Postlar (Posts)
                if (!await context.Posts.AnyAsync())
                {
                    var posts = GeneratePosts(50, categoriesFromDb, usersFromDb);
                    await context.Posts.AddRangeAsync(posts);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Postlar eklendi.");
                }
                else
                {
                    Console.WriteLine("⚠️ Postlar zaten mevcut, tekrar eklenmedi.");
                }

                // 6. Postları ve Etiketleri (Tags) yeniden çek
                var postsFromDb = await context.Posts.ToListAsync();
                var tagsFromDb = await context.Tags.ToListAsync();

                // 7. Post-Tag İlişkileri (PostTags)
                if (!await context.PostTags.AnyAsync())
                {
                    var postTags = GeneratePostTags(postsFromDb, tagsFromDb);
                    await context.PostTags.AddRangeAsync(postTags);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Post-Tag ilişkileri eklendi.");
                }

                // 8. Yorumlar (Comments)
                if (!await context.Comments.AnyAsync())
                {
                    var comments = GenerateComments(200, postsFromDb, usersFromDb);
                    await context.Comments.AddRangeAsync(comments);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Yorumlar eklendi.");
                }

                Console.WriteLine("🎉 Tüm Fake Data Başarıyla Eklendi!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Hata: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"🔥 Inner Exception: {ex.InnerException.Message}");
                }
            }
        }

        private static List<AppUser> GenerateUsers(int count)
        {
            return new Faker<AppUser>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.UserName, f => f.Internet.UserName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.PasswordHash, f => f.Internet.Password())
                .RuleFor(u => u.FullName, f => f.Name.FullName())
                .RuleFor(u => u.Role, f => f.PickRandom<Roles>())
                .Generate(count);
        }

        private static List<Category> GenerateCategories(int count)
        {
            return new Faker<Category>()
                .RuleFor(c => c.Id, f => Guid.NewGuid())
                .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
                .RuleFor(c => c.Slug, f => f.Lorem.Slug())
                .RuleFor(c => c.Description, f => f.Lorem.Paragraph())
                .Generate(count);
        }

        private static List<Tag> GenerateTags(int count)
        {
            return new Faker<Tag>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Lorem.Word())
                .RuleFor(t => t.Slug, f => f.Lorem.Slug())
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .Generate(count);
        }

        private static List<Post> GeneratePosts(int count, List<Category> categories, List<AppUser> users)
        {
            var postFaker = new Faker<Post>()
                .RuleFor(p => p.Id, f => Guid.NewGuid())
                .RuleFor(p => p.Title, f => f.Lorem.Sentence())
                .RuleFor(p => p.Slug, (f, p) => GenerateSlug(p.Title))
                .RuleFor(p => p.Excerpt, f => f.Lorem.Sentence())
                .RuleFor(p => p.Content, f => f.Lorem.Paragraphs(3))
                .RuleFor(p => p.FeaturedImage, f => f.Image.PicsumUrl())
                .RuleFor(p => p.ViewCount, f => f.Random.Number(0, 1000))
                .RuleFor(p => p.UserId, f => f.PickRandom(users)?.Id ?? users.First().Id)
                .RuleFor(p => p.CategoryId, f => f.PickRandom(categories)?.Id ?? categories.First().Id)
                .RuleFor(p => p.CreatedAt, f => f.Date.Between(DateTime.UtcNow.AddYears(-2), DateTime.UtcNow))
            .RuleFor(p => p.VideoUrl, f => f.Random.Bool(0.7f) ? f.Internet.Url() : "https://example.com/default-video.mp4");


            var posts = postFaker.Generate(count);
            foreach (var post in posts)
            {
                Console.WriteLine($"Post Title: {post.Title} -> Slug: {post.Slug}");
            }
            return posts;
        }

        private static List<PostTag> GeneratePostTags(List<Post> posts, List<Tag> tags)
        {
            var postTags = new List<PostTag>();

            foreach (var post in posts)
            {
                var selectedTags = tags.OrderBy(x => Guid.NewGuid()).Take(3).ToList();
                foreach (var tag in selectedTags)
                {
                    postTags.Add(new PostTag
                    {
                        PostId = post.Id,
                        TagId = tag.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            return postTags;
        }

        private static List<Comment> GenerateComments(int count, List<Post> posts, List<AppUser> users)
        {
            var commentFaker = new Faker<Comment>()
                .RuleFor(c => c.Id, f => Guid.NewGuid())
                .RuleFor(c => c.Content, f => f.Lorem.Paragraph())
                .RuleFor(c => c.PostId, f => f.PickRandom(posts)?.Id ?? posts.First().Id)
                .RuleFor(c => c.UserId, f => f.PickRandom(users)?.Id)
                .RuleFor(c => c.AuthorName, f => f.Name.FullName())
                .RuleFor(c => c.AuthorEmail, f => f.Internet.Email())
                .RuleFor(c => c.CreatedAt, f => f.Date.Between(DateTime.UtcNow.AddYears(-1), DateTime.UtcNow));

            return commentFaker.Generate(count).Where(c => posts.Any(p => p.Id == c.PostId)).ToList();
        }

        private static string GenerateSlug(string title)
        {
            return Regex.Replace(title.ToLower().Replace(" ", "-"), @"[^a-z0-9-]", "");
        }
    }
}
