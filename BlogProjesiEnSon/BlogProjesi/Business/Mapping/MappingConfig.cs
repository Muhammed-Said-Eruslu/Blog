using Business.DTOs.CommentsDTOs;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Mapping
{
    public class MappingConfig
    {
        public static void Configure()
        {
            TypeAdapterConfig<Comment, CommentDTO>.NewConfig()
                .Map(dest => dest.AuthorProfileImage, src => src.User != null ? src.User.ProfileImage : "/images/default-avatar.png")
                .Map(dest => dest.UserName, src => src.User != null ? src.User.UserName : "Bilinmiyor");
        }
    }
}
