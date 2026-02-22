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

            CreateMap<ApplicationUser, UserDto>();

            CreateMap<UserUpdateDto, ApplicationUser>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UserChangePassword, ApplicationUser>();

        }
    }
}