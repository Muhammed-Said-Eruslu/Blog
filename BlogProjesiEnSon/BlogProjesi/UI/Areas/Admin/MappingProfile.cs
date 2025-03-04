using Business.DTOs.PostDTOs;
using Business.DTOs.PostTagDTOs;
using Domain.Entites;
using Mapster;

namespace UI.Areas.Admin
{
    public class MappingProfile
    {
        public static void ConfigureMappings()
        {
            TypeAdapterConfig<Post, PostDetailDTO>.NewConfig()
                .Map(dest => dest.Tags, src => src.PostTag.Select(pt => new PostTagDTO
                {
                    Id = pt.Id,
                    PostId = pt.PostId,
                    TagId = pt.TagId,
                    IsActive = pt.IsActive,
                    CreatedAt = pt.CreatedAt,
                    UpdatedAt = pt.UpdatedAt,
                    PostTitle = pt.Post.Title,
                    TagName = pt.Tag.Name  // Tag adını açıkça set ediyoruz
                }).ToList());
        }
    }
}
