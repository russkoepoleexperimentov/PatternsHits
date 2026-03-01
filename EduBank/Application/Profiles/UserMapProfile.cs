using Application.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Profiles
{
    public class UserMapProfile : Profile
    {
        public UserMapProfile()
        {

            CreateMap<UserRegisterDto, ApplicationUser>();

            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src =>
                    src.LockoutEnabled && src.LockoutEnd.HasValue && src.LockoutEnd > DateTimeOffset.UtcNow))
                .ForMember(dest => dest.Roles, opt => opt.Ignore());

            CreateMap<UserUpdateDto, ApplicationUser>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UserChangePassword, ApplicationUser>();

        }
    }
}