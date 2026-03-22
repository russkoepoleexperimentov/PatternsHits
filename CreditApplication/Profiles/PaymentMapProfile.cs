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
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CreditId, opt => opt.MapFrom(src => src.CreditId))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreateDateTime))
                .ForMember(dest => dest.ProcessedAt, opt => opt.MapFrom(src => src.ProcessedAt))
                .ForMember(dest => dest.OriginalAmount, opt => opt.MapFrom(src => src.OriginalAmount))
                .ForMember(dest => dest.OriginalCurrency, opt => opt.MapFrom(src => src.OriginalCurrency))
                .ForMember(dest => dest.ExchangeRate, opt => opt.MapFrom(src => src.ExchangeRate))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Credit != null ? src.Credit.Currency : src.OriginalCurrency ?? "RUB"));

            CreateMap<CreatePaymentRequest, Payment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreditId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ProcessedAt, opt => opt.Ignore())
                .ForMember(dest => dest.FailureReason, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDateTime, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ModifyDateTime, opt => opt.Ignore());

            CreateMap<UpdatePaymentStatusRequest, Payment>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ProcessedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
