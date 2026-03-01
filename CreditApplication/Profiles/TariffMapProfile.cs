using AutoMapper;
using CreditApplication.Dtos;
using CreditDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditApplication.Profiles
{
    public class TariffProfile : Profile
    {
        public TariffProfile()
        {
            CreateMap<Tariff, TariffDto>();
            CreateMap<CreateTariffRequest, Tariff>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDateTime, opt => opt.Ignore())
                .ForMember(dest => dest.ModifyDateTime, opt => opt.Ignore());
            CreateMap<UpdateTariffRequest, Tariff>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDateTime, opt => opt.Ignore())
                .ForMember(dest => dest.ModifyDateTime, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
