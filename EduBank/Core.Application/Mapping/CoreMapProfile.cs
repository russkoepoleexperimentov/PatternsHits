using Core.Application.Dtos;
using AutoMapper;
using Core.Domain;
using System.Linq.Expressions;

namespace Core.Application.Mapping
{
    public class CoreMapProfile : Profile
    {
        public CoreMapProfile()
        {
            CreateMap<Account, AccountDto>();

            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.DisplayType, opt => opt.MapFrom(src => CalculateTransactionType(src)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(x => x.CreateDateTime));

            CreateMap<CreateTransactionDto, Transaction>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => TransactionStatus.Pending))
                .ForMember(dest => dest.ResolutionMessage, opt => opt.MapFrom<string?>(_ => null))
                .ForMember(dest => dest.ResolvedAt, opt => opt.MapFrom<DateTime?>(_ => null))
                .ForMember(dest => dest.CreateDateTime, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<Transaction, AccountTransactionDto>()
                .ForMember(dest => dest.Amount, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? src.ResolutionMessage ?? "Транзакция"))
                .ForMember(dest => dest.ResolutionMessage, opt => opt.MapFrom(src => src.ResolutionMessage))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreateDateTime))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
        }

        private static TransactionType CalculateTransactionType(Transaction src)
        {
            if (src.SourceType == TransactionObjectType.RealWorld && src.TargetType == TransactionObjectType.Account) return TransactionType.Deposit;
            if (src.SourceType == TransactionObjectType.Account && src.TargetType == TransactionObjectType.RealWorld) return TransactionType.Withdrawal;
            if (src.SourceType == TransactionObjectType.Account && src.TargetType == TransactionObjectType.Account) return TransactionType.Transfer;
            if (src.SourceType == TransactionObjectType.Credit) return TransactionType.CreditIncoming;
            if (src.TargetType == TransactionObjectType.Credit) return TransactionType.CreditPayment;
            return TransactionType.Unclassified;
        }
    }
}
