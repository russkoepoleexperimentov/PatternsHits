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
    public class CreditProfile : Profile
    {
        public CreditProfile()
        {
            CreateMap<Credit, CreditDto>();

            CreateMap<CreateCreditRequest, Credit>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RemainingDebt, opt => opt.Ignore()) 
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedAmount, opt => opt.Ignore())
                .ForMember(dest => dest.RejectionReason, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ClosedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDateTime, opt => opt.Ignore())
                .ForMember(dest => dest.ModifyDateTime, opt => opt.Ignore());

            CreateMap<ApproveCreditRequest, Credit>()
                .ForMember(dest => dest.ApprovedAmount, opt => opt.MapFrom(src => src.ApprovedAmount))
                .ForMember(dest => dest.RejectionReason, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore()) 
                .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<RejectCreditRequest, Credit>()
                .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.Reason))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
