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

            CreateMap<Transaction, TransactionDto>();

            CreateMap<CreateTransactionDto, Transaction>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => TransactionStatus.Pending))
                .ForMember(dest => dest.ResolutionMessage, opt => opt.MapFrom<string?>(_ => null))
                .ForMember(dest => dest.ResolvedAt, opt => opt.MapFrom<DateTime?>(_ => null));
        }
    }
}
