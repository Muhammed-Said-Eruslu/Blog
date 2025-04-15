using Business.DTOs.CommentsDTOs;
using Business.DTOs.PostDTOs;
using Domain.Entites;
using Domain.Enums;
using Domain.Utilities.Concretes;
using Domain.Utilities.Interfaces;
using Infrastructure.Repositories.CategoryRepository;
using Infrastructure.Repositories.CommentRepository;
using Infrastructure.Repositories.PostRepository;
using Infrastructure.Repositories.PostTagRepository;
using Infrastructure.Repositories.TagRepository;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Domain.Core.İnterfaces;
using Business.DTOs.TagDTOs;
using Business.DTOs.PostTagDTOs;

namespace Business.Services.PostServices
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ITagRepostiry _tagRepository;
        private readonly ICategoryRepository _categoryRepository;

        public PostService(IPostRepository postRepository, ICommentRepository commentRepository, ITagRepostiry tagRepository, ICategoryRepository categoryRepository)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _tagRepository = tagRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IResult> AddAsync(PostCreateDTO postCreateDTO)
        {
            if (await _postRepository.AnyAsync(p => p.Title.ToLower() == postCreateDTO.Title.ToLower()))
                return new ErrorResult("A post with the same title already exists.");

            var newPost = postCreateDTO.Adapt<Post>();
            newPost.CreatedAt = DateTime.UtcNow;

            await _postRepository.AddAsync(newPost);
            await _postRepository.SaveChangeAsync();

            return new SuccessResult("Post added successfully.");
        }

        public async Task<IResult> AddCommentToPostAsync(Guid postId, CommentCreateDTO commentCreateDTO)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
                return new ErrorResult("Post not found.");

            var newComment = commentCreateDTO.Adapt<Comment>();
            newComment.PostId = postId;
            newComment.CreatedDate = DateTime.UtcNow;

            await _commentRepository.AddAsync(newComment);
            await _commentRepository.SaveChangeAsync();

            return new SuccessResult("Comment added successfully.");
        }

        public async Task<IResult> AddTagsToPostAsync(Guid postId, List<Guid> tagIds)
        {
            // Post'u bul
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
                return new ErrorResult("Post not found.");

            // tagIds null veya boşsa hata döndür
            if (tagIds == null || !tagIds.Any())
                return new ErrorResult("No tags provided.");

            // Tüm etiketleri tek bir sorguda al
            var tags = await _tagRepository.GetAllAsync(t => tagIds.Contains(t.Id));

            // Etiketleri post'a ekle
            foreach (var tag in tags)
            {
                // Aynı etiket zaten eklenmiş mi kontrol et
                if (!post.PostTag.Any(pt => pt.TagId == tag.Id))
                {
                    post.PostTag.Add(new PostTag { PostId = postId, TagId = tag.Id });
                }
            }

            // Post'u güncelle
            await _postRepository.UpdateAsync(post);
            await _postRepository.SaveChangeAsync();

            return new SuccessResult("Tags added to post successfully.");
        }

        public async Task<IResult> BulkDeletePostsAsync(List<Guid> postIds)
        {
            var postsToDelete = await _postRepository.GetAllAsync(p => postIds.Contains(p.Id));
            if (!postsToDelete.Any())
                return new ErrorResult("No posts found to delete.");

            _postRepository.RemoveRange(postsToDelete);
            await _postRepository.SaveChangeAsync();

            return new SuccessResult("Posts deleted successfully.");
        }

        public async Task<IResult> DeleteAsync(Guid id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
                return new ErrorResult("Post not found.");

            await _postRepository.DeleteAsync(post);
            await _postRepository.SaveChangeAsync();

            return new SuccessResult("Post deleted successfully.");
        }

        public async Task<IDataResult<List<PostListDTO>>> GetAllAsync()
        {
            var posts = await _postRepository.GetAllAsync(
                predicate: p => p.User.Role == Roles.Admin,
                include: query => query.Include(p => p.User).Include(p => p.Category)
            );

            var postListDTOs = posts.Select(p => new PostListDTO
            {
                Id = p.Id,
                Title = p.Title,
                VideoUrl = p.VideoUrl,
                Excerpt = p.Excerpt,
                FeaturedImage = p.FeaturedImage,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "Bilinmiyor",
                UserId = p.Id,
                Slug = p.Slug,
                FullName = p.User != null ? p.User.UserName : "Bilinmiyor"
            }).ToList();

            if (!postListDTOs.Any())
                return new ErrorDataResult<List<PostListDTO>>(postListDTOs, "No posts found.");

            return new SuccessDataResult<List<PostListDTO>>(postListDTOs, "Posts retrieved successfully.");
        }



        public async Task<IDataResult<T>> GetPostWithIncludesAsync<T>(Guid postId, Func<IQueryable<T>, IQueryable<T>> include) where T : class, IEntity
        {

            var post = await _postRepository.GetAsyncWithIncludes<T>(p => p.Id == postId, include);

            if (post == null)
                return new ErrorDataResult<T>(null, "Post not found.");

            return new SuccessDataResult<T>(post, "Post details retrieved successfully.");
        }

        public async Task<IDataResult<PostDTO>> GetByIdAsync(Guid postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
                return new ErrorDataResult<PostDTO>(null, "Gönderi bulunamadı.");

            var dto = post.Adapt<PostDTO>(); // LikeCount, ViewCount vs. var
            return new SuccessDataResult<PostDTO>(dto, "Gönderi başarıyla bulundu.");
        }

        public async Task<IDataResult<List<PostListDTO>>> GetPopularPostsAsync(int count)
        {
            // Sıralama: en çok görüntülenme, istersen (Like + View + Comment)
            var posts = await _postRepository.GetAllAsync(
                include: q => q
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Include(p => p.Comments),
                orderBy: q => q.OrderByDescending(p => p.ViewCount),
                take: count
            );

            var postListDTOs = posts.Select(p => new PostListDTO
            {
                Id = p.Id,
                Title = p.Title,
                Excerpt = p.Excerpt,
                FeaturedImage = p.FeaturedImage,
                CategoryName = p.Category?.Name ?? "",
                Slug = p.Slug,
                LikeCount = p.LikeCount,
                ViewCount = p.ViewCount,
                CommentCount = p.Comments.Count()
            }).ToList();

            return new SuccessDataResult<List<PostListDTO>>(postListDTOs, "Popular posts retrieved successfully.");
        }



        public async Task<IDataResult<PostDTO>> GetPostBySlugAsync(string slug)
        {
            var post = await _postRepository.GetAsync(
                predicate: p => p.Slug == slug,
                include: query => query
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Include(p => p.Comments).ThenInclude(c => c.User)
                    .Include(p => p.PostTag).ThenInclude(pt => pt.Tag)
            );

            if (post == null)
                return new ErrorDataResult<PostDTO>(null, "Post not found.");

            var postDTO = post.Adapt<PostDTO>();

            // Kategori bilgisi
            postDTO.CategoryName = post.Category?.Name;

            // Etiketler (PostTagDTO'ya dönüştür)
            postDTO.PostTags = post.PostTag.Select(pt => new PostTagDTO
            {
                PostId = pt.PostId,
                TagId = pt.TagId,
                TagName = pt.Tag.Adapt<TagDTO>().ToString() // TagDTO'ya dönüştür
            }).ToList();

            // Yorumlar (son 10 yorum)
            postDTO.Comments = post.Comments
                .OrderByDescending(c => c.CreatedAt)
                .Take(10)
                .Select(c => c.Adapt<CommentDTO>())
                .ToList();

            return new SuccessDataResult<PostDTO>(postDTO, "Post retrieved successfully.");
        }


        public async Task<IDataResult<PostDetailDTO>> GetPostDetailsAsync(Guid postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
                return new ErrorDataResult<PostDetailDTO>(null, "Post not found.");

            var postDetailDTO = post.Adapt<PostDetailDTO>();
            return new SuccessDataResult<PostDetailDTO>(postDetailDTO, "Post details retrieved successfully.");
        }

        public async Task<IDataResult<PostDetailDTO>> GetPostDetailsWithIncludesAsync(Guid postId)
        {
            var post = await _postRepository.GetAsyncWithIncludes<Post>(
                p => p.Id == postId,
                query => query
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .Include(p => p.Comments).ThenInclude(c => c.User)
                    .Include(p => p.PostTag).ThenInclude(pt => pt.Tag)
            );

            if (post == null)
                return new ErrorDataResult<PostDetailDTO>(null, "Post not found.");

            // Mapster ile dönüştürme
            var postDetailDTO = post.Adapt<PostDetailDTO>();

            // Manuel olarak yazar adını ekleyelim
            postDetailDTO.AuthorName = post.User != null ? post.User.UserName : "Bilinmiyor";

            return new SuccessDataResult<PostDetailDTO>(postDetailDTO, "Post details retrieved successfully.");
        }




        public async Task<IDataResult<List<PostListDTO>>> GetPostsByCategoryIdAsync(Guid categoryId)
        {
            var posts = await _postRepository.GetAllAsync(p => p.CategoryId == categoryId, include: q => q.Include(x => x.User));

            var postListDTOs = posts.Select(p =>
            {
                var postListDTO = p.Adapt<PostListDTO>();
                postListDTO.FullName = p.User.FullName; // Burada kullanıcı adı atanıyor
                return postListDTO;
            }).ToList();

            if (!postListDTOs.Any())
                return new ErrorDataResult<List<PostListDTO>>(postListDTOs, "No posts found for this category.");

            return new SuccessDataResult<List<PostListDTO>>(postListDTOs, "Posts retrieved successfully.");
        }

        public async Task<IDataResult<List<PostListDTO>>> GetPostsByUserIdAsync(Guid userId)
        {
            var posts = await _postRepository.GetAllAsync(p => p.UserId == userId);
            var postListDTOs = posts.Select(p => p.Adapt<PostListDTO>()).ToList();

            if (!postListDTOs.Any())
                return new ErrorDataResult<List<PostListDTO>>(postListDTOs, "No posts found for this user.");

            return new SuccessDataResult<List<PostListDTO>>(postListDTOs, "Posts retrieved successfully.");
        }

        public async Task<IDataResult<PostStatisticsDTO>> GetPostStatisticsAsync()
        {
            // Toplam yazı sayısını al
            var totalPosts = await _postRepository.CountAsync();

            // Toplam yorum sayısını al (_commentRepository üzerinden)
            var totalComments = await _commentRepository.CountAsync();

            // Toplam etiket sayısını al (_tagRepository üzerinden)
            var allTags = await _tagRepository.GetAllAsync();
            var totalTags = allTags.Count();

            // Kategorilere göre yazı sayısını al
            var categories = await _categoryRepository.GetAllAsync();
            var postCountByCategory = categories
                .Select(c => new CategoryPostCountDTO
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    PostCount = c.Posts != null ? c.Posts.Count : 0
                })
                .ToList();

            // En popüler yazıları al (en çok görüntülenen 5 yazı)
            var popularPosts = await _postRepository.GetAllAsync(
                orderBy: q => q.OrderByDescending(p => p.ViewCount),
                take: 5
            );

            // En çok yorum alan yazıları al:
            // Öncelikle, yorumları içeren postları çekiyoruz.
            var postsWithComments = await _postRepository.GetAllAsync(
                predicate: p => p.Comments != null,
                include: query => query.Include(p => p.Comments)
            );
            var mostCommentedPosts = postsWithComments
                .OrderByDescending(p => p.Comments?.Count ?? 0)
                .Take(5)
                .ToList();

            // İstatistik DTO'sunu dolduralım
            var postStatisticsDTO = new PostStatisticsDTO
            {
                TotalPosts = totalPosts,
                TotalComments = totalComments,
                TotalTags = totalTags,
                PostCountByCategory = postCountByCategory,
                PopularPosts = popularPosts.Select(p => p.Adapt<PostListDTO>()).ToList(),
                MostCommentedPosts = mostCommentedPosts.Select(p => p.Adapt<PostListDTO>()).ToList()
            };

            return new SuccessDataResult<PostStatisticsDTO>(postStatisticsDTO, "Post statistics retrieved successfully.");
        }



        public async Task<IResult> RemoveTagsFromPostAsync(Guid postId, List<Guid> tagIds)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
                return new ErrorResult("Post not found.");

            var tagsToRemove = post.PostTag.Where(pt => tagIds.Contains(pt.TagId)).ToList();
            foreach (var tag in tagsToRemove)
            {
                post.PostTag.Remove(tag);
            }

            await _postRepository.UpdateAsync(post);
            await _postRepository.SaveChangeAsync();

            return new SuccessResult("Tags removed from post successfully.");
        }

        public async Task<IDataResult<List<PostListDTO>>> SearchPostsAsync(string searchTerm)
        {
            var posts = await _postRepository.GetAllAsync(p =>
                p.Title.Contains(searchTerm) || p.Content.Contains(searchTerm));

            var postListDTOs = posts.Select(p => p.Adapt<PostListDTO>()).ToList();

            if (!postListDTOs.Any())
                return new ErrorDataResult<List<PostListDTO>>(postListDTOs, "No posts found.");

            return new SuccessDataResult<List<PostListDTO>>(postListDTOs, "Posts retrieved successfully.");
        }

        public async Task<IResult> UpdateAsync(PostUpdateDTO postUpdateDTO)
        {
            try
            {
                var existingPost = await _postRepository.GetByIdAsync(postUpdateDTO.Id);
                if (existingPost == null)
                    return new ErrorResult("Post not found.");

                // Orijinal RowVersion'i sakla
                var originalRowVersion = existingPost.RowVersion;

                postUpdateDTO.Adapt(existingPost);
                existingPost.UpdatedDate = DateTime.UtcNow;

                // Etiket işlemleri
                existingPost.PostTag.Clear();
                if (postUpdateDTO.TagIds != null && postUpdateDTO.TagIds.Any())
                {
                    var tags = await _tagRepository.GetAllAsync(t => postUpdateDTO.TagIds.Contains(t.Id));
                    foreach (var tag in tags)
                    {
                        existingPost.PostTag.Add(new PostTag { PostId = existingPost.Id, TagId = tag.Id });
                    }
                }

                try
                {
                    await _postRepository.UpdateAsync(existingPost);
                    await _postRepository.SaveChangeAsync();

                    return new SuccessResult("Post updated successfully.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    if (databaseValues == null)
                    {
                        return new ErrorResult("Bu post başka bir kullanıcı tarafından silindi.");
                    }
                    else
                    {
                        var databasePost = (Post)databaseValues.ToObject();
                        return new ErrorResult($"Bu kayıt {databasePost.UpdatedDate} tarihinde başka bir kullanıcı tarafından değiştirildi.");
                    }
                }
            }
            catch (Exception ex)
            {
                return new ErrorResult("Post güncellenirken bir hata oluştu: " + ex.Message);
            }
        }

        public async Task<IResult> UpdatePostStatusAsync(Guid postId, Status status)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
                return new ErrorResult("Post not found.");

            post.Status = status;
            await _postRepository.UpdateAsync(post);
            await _postRepository.SaveChangeAsync();

            return new SuccessResult("Post status updated successfully.");
        }

        public async Task<IResult> UpdateVideoUrlAsync(Guid postId, string videoUrl)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                return new ErrorResult("Yazı bulunamadı.");
            }

            post.VideoUrl = videoUrl;
            await _postRepository.UpdateAsync(post);

            return new SuccessResult("Video URL'si başarıyla güncellendi.");
        }

        public async Task<IDataResult<List<string>>> GetAllSlugsAsync()
        {
            var slugs = await _postRepository.GetAllAsync(orderBy: null, take: null);

            var slugList = slugs
                            .Where(p => !string.IsNullOrEmpty(p.Slug))
                            .Select(p => p.Slug)
                            .ToList();

            return new DataResult<List<string>>(slugList, true, "Slugs retrieved successfully");


        }

        public async Task<IDataResult<List<PostListDTO>>> GetAllWithCountAsync(Guid userId)
        {
            var posts = await _postRepository.GetAllAsync(
                include: query => query
                    .Include(p => p.Comments)
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Include(p => p.LikedUsers)
            );

            var postListDTOs = posts.Select(p => new PostListDTO
            {
                Id = p.Id,
                Title = p.Title,
                VideoUrl = p.VideoUrl,
                Excerpt = p.Excerpt,
                FeaturedImage = p.FeaturedImage,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "Bilinmiyor",
                UserId = p.UserId ?? Guid.Empty,
                FullName = p.User != null ? p.User.FullName : "Bilinmiyor",
                Slug = p.Slug,
                ViewCount = p.ViewCount,
                LikeCount = p.LikeCount,
                IsLiked = p.LikedUsers.Any(lu => lu.UserId == userId),
                CommentCount = p.Comments.Count(),
                CreatedDate = p.CreatedDate
            }).ToList();

            return new SuccessDataResult<List<PostListDTO>>(postListDTOs, "Posts retrieved successfully.");
        }
        public async Task<IResult> IncreaseViewCountAsync(Guid postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
                return new ErrorResult("Post bulunamadı.");

            post.ViewCount++;  // DB'de kalıcı
            await _postRepository.UpdateAsync(post);
            await _postRepository.SaveChangeAsync();

            return new SuccessResult("Görüntülenme sayısı arttırıldı.");
        }
        public async Task<IResult> IncreaseLikeCountAsync(Guid postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
                return new ErrorResult("Post bulunamadı.");

            post.LikeCount++; // DB'de kalıcı
            await _postRepository.UpdateAsync(post);
            await _postRepository.SaveChangeAsync();

            return new SuccessResult("Beğeni sayısı arttırıldı.");
        }

        public async Task<IDataResult<T>> GetPostWithIncludesAsync<T>(string slug, Func<IQueryable<T>, IQueryable<T>> include) where T : class, Domain.Core.İnterfaces.IEntity
        {
            var query = await _postRepository.GetAllWithIncludesAsync<T>(
                p => EF.Property<string>(p, "Slug") == slug,
                include
            );

            var post = query.FirstOrDefault();
            return post != null
                ? new SuccessDataResult<T>(post, "Post başarıyla getirildi.")
                : new ErrorDataResult<T>("Post bulunamadı.");
        }
        public async Task<IDataResult<List<PostListDTO>>> GetAllWithCountAsync(string searchQuery)
        {
            var posts = await _postRepository.GetAllAsync(
                predicate: p => string.IsNullOrEmpty(searchQuery) ||
                                p.Title.Contains(searchQuery) ||
                                p.Excerpt.Contains(searchQuery),
                include: query => query
                    .Include(p => p.Comments)
                    .Include(p => p.User)
                    .Include(p => p.Category)
            );

            // DTO'ya map
            var postListDTOs = posts.Select(p => new PostListDTO
            {
                Id = p.Id,
                Title = p.Title,
                VideoUrl = p.VideoUrl,
                Excerpt = p.Excerpt,
                FeaturedImage = p.FeaturedImage,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "Bilinmiyor",
                UserId = p.UserId ?? Guid.Empty,
                FullName = p.User != null ? p.User.FullName : "Bilinmiyor",
                Slug = p.Slug,
                // Kalıcı sayılar
                ViewCount = p.ViewCount,
                LikeCount = p.LikeCount,
                CommentCount = p.Comments.Count(),
                CreatedDate = p.CreatedDate // Bu alan veritabanında varsa
            }).ToList();

            return new SuccessDataResult<List<PostListDTO>>(postListDTOs, "Filtered posts retrieved successfully.");
        }

        

        public async Task<IDataResult<List<PostListDTO>>> GetRelatedTagPostsAsync(Guid postId, int count = 5)
        {
            var currentPost = await _postRepository.GetByIdAsync(postId);
            if (currentPost == null)
                return new ErrorDataResult<List<PostListDTO>>(null, "Post bulunamadı."); // ✅ Fix

            var tagIds = currentPost.PostTag.Select(pt => pt.TagId).ToList();

            // Aynı etiketlere sahip postlar (mevcut post hariç)
            var posts = await _postRepository.GetAllAsync(
                predicate: p => p.Id != postId && p.PostTag.Any(pt => tagIds.Contains(pt.TagId)),
                include: q => q.Include(p => p.User).Include(p => p.Category),
                take: count
            );

            var dtos = posts.Select(p => p.Adapt<PostListDTO>()).ToList();
            return new SuccessDataResult<List<PostListDTO>>(dtos, "Related posts retrieved successfully.");
        }

        // Benzer Kategorideki Postlar
        public async Task<IDataResult<List<PostListDTO>>> GetRelatedCategoryPostsAsync(Guid postId, int count = 5)
        {
            var currentPost = await _postRepository.GetByIdAsync(postId);
            if (currentPost == null)
                return new ErrorDataResult<List<PostListDTO>>(null, "Post bulunamadı."); // ✅ Fix

            var categoryId = currentPost.CategoryId;

            // Aynı kategorideki postlar (mevcut post hariç)
            var posts = await _postRepository.GetAllAsync(
                predicate: p => p.CategoryId == categoryId && p.Id != postId,
                include: q => q.Include(p => p.User).Include(p => p.Category),
                take: count
            );

            var dtos = posts.Select(p => p.Adapt<PostListDTO>()).ToList();
            return new SuccessDataResult<List<PostListDTO>>(dtos, "Related posts retrieved successfully.");
        }

        public async Task<AppUser> GetUserWithLikedPostsAsync(Guid userId)
        {
            return await _postRepository.GetUserWithLikedPostsAsync(userId);
        }

        public async Task<IDataResult<Post>> ToggleLikeAsync(Guid userId, Guid postId)
        {
            // Kullanıcıyı ve postu çek
            var user = await _postRepository.GetUserWithLikedPostsAsync(userId);
            var post = await _postRepository.GetByIdAsync(postId);

            if (user == null)
                return new ErrorDataResult<Post>("Kullanıcı bulunamadı.");

            if (post == null)
                return new ErrorDataResult<Post>("Post bulunamadı.");

            // Kullanıcının bu postu beğenip beğenmediğini kontrol et
            var existingLike = await _postRepository.GetAppUserPostAsync(userId, postId);

            if (existingLike != null)
            {
                // Eğer daha önce beğenmişse, beğeniyi kaldır
                await _postRepository.RemoveLikeAsync(existingLike);
                post.LikeCount--;
            }
            else
            {
                // Eğer beğenmemişse, beğeni ekle
                await _postRepository.AddLikeAsync(new AppUserPost { UserId = userId, PostId = postId });
                post.LikeCount++;
            }

            // Değişiklikleri kaydet
            await _postRepository.SaveChangeAsync();

            return new SuccessDataResult<Post>(post, "Beğeni durumu değiştirildi.");
        }



    }
}
