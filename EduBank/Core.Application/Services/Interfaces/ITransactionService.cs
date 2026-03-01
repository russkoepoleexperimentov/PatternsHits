
using Common.Contracts;
using Core.Application.Dtos;
using Core.Domain;

namespace Core.Application.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionDto> InitializeTransactionAsync(CreateTransactionDto dto, Guid currentUserId);
        Task<TransactionDto> GetTransactionByIdAsync(Guid id, Guid currentUserId);
        Task<DepositFundsResponse> ProcessDepositFund(DepositFundsCommand command);
    }
}
