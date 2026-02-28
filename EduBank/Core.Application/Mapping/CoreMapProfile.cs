using Core.Application.Dtos;
using AutoMapper;
using Core.Domain;

namespace Core.Application.Mapping
{
    public class CoreMapProfile : Profile
    {
        public CoreMapProfile()
        {
            CreateMap<Account, AccountDto>();
            CreateMap<CreateAccountDto, Account>()
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.InitialBalance));

            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString().ToLower()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateTransactionDto, Transaction>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => TransactionStatus.Pending));
        }
    }
}
