using Core.Application.Dtos;
using Core.Domain;

namespace Core.Application.Services.Interfaces
{
    public interface IAccountService
    {
        Task<List<AccountDto>> GetAccountsAsync(Guid? userId, Guid currentUserId);
        Task<AccountDto> CreateAccountAsync(Guid currentUserId, CreateAccountDto dto);
        Task<AccountDto> GetAccountByIdAsync(Guid id, Guid currentUserId);
        Task CloseAccountAsync(Guid id, Guid currentUserId);
        Task<List<TransactionDto>> GetAccountTransactionsAsync(Guid accountId, DateTime? from, DateTime? to, Guid currentUserId);
        Task<Account> GetAccountFromDbAsync(Guid value, Guid? currentUserId);
    }
}
