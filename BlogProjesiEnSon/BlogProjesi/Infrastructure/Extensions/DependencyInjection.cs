using Domain.Entites;
using Infrastructure.AppContext;
using Infrastructure.DataAccess.Interface;
using Infrastructure.Repositories.CategoryRepository;
using Infrastructure.Repositories.CommentRepository;
using Infrastructure.Repositories.ContactRepository;
using Infrastructure.Repositories.MailRepository;
using Infrastructure.Repositories.PhotoRepository;
using Infrastructure.Repositories.PostRepository;
using Infrastructure.Repositories.PostTagRepository;
using Infrastructure.Repositories.SubscriberRepository;
using Infrastructure.Repositories.TagRepository;
using Infrastructure.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext< DbContext,AppDbContext>(options =>
            {
                options.UseLazyLoadingProxies();

                options.UseSqlServer(configuration.GetConnectionString("AppConnectionString"), sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });
            });

            // Identity servisini ekle
            services.AddIdentity<AppUser, IdentityRole<Guid>>()
     .AddEntityFrameworkStores<AppDbContext>()
     .AddDefaultTokenProviders();


            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IPostTagRepository, PostTagRepository>();
            services.AddScoped<ITagRepostiry, TagRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IAsyncSendMailRepository, MailRepository>();
            services.AddScoped<IContactRepistory, ContactRepository>();
            services.AddScoped<ISubscriberRepository, SubscriberRepository>();
            services.AddScoped<IPhotoRepository, PhotoRepository>();
            
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                FakeDataGenerator.SeedAsync(context).GetAwaiter().GetResult();
            }

            return services;
        }
    }
}
 