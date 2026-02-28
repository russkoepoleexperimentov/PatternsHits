using System.Transactions;
using Core.Application.Dtos;

namespace Core.Application.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionDto> InitializeTransactionAsync(CreateTransactionDto dto, Guid currentUserId);
        Task<TransactionDto> GetTransactionByIdAsync(Guid id, Guid currentUserId);
        Task<TransactionDto> ResolveTransactionAsync(Guid id, TransactionStatus status, string message);
    }
}
