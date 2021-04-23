using AutoMapper;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src =>
                  src.Photos.FirstOrDefault(x => x.IsMain == true).Url))
                .ForMember(dest => dest.age, opt => opt.MapFrom(src =>
                 src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderPhotoUrl, opt => opt.MapFrom(src =>
                 src.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.RecepientPhotoUrl, opt => opt.MapFrom(src =>
                 src.Recepient.Photos.FirstOrDefault(x => x.IsMain).Url));

            //CreateMap<AppUser, PhotoForApprovalDto>()
            //         .ForMember(dest => dest.Url, opt => opt.MapFrom(src =>
            //     src.Photos.FirstOrDefault(x => x.IsMain== false).Url));


        }
        
    }
}
