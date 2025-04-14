using Business.Services.AccountService;
using Business.Services.CategoryServices;
using Business.Services.CommentsServices;
using Business.Services.ContactServices;
using Business.Services.PhotoServices;
using Business.Services.PostServices;
using Business.Services.PostTagServices;
using Business.Services.SubscriberService;
using Business.Services.TagServices;
using Infrastructure.Repositories.PhotoRepository;
using Infrastructure.Repositories.TagRepository;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Extentions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IPostTagService, PostTagService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<ISubscriberService, SubscriberService>();
            services.AddScoped<IPhotoService, PhotoService>();

            return services;
        }
    }
}
