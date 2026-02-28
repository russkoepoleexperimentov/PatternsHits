using Core.Application.Dtos;

namespace Core.Application.Services.Interfaces
{
    public interface IAccountService
    {
        Task<List<AccountDto>> GetAccountsAsync(Guid? userId, Guid currentUserId);
        Task<AccountDto> CreateAccountAsync(CreateAccountDto dto, Guid currentUserId);
        Task<AccountDto> GetAccountByIdAsync(Guid id, Guid currentUserId);
        Task CloseAccountAsync(Guid id, Guid currentUserId);
        Task<List<TransactionDto>> GetAccountTransactionsAsync(Guid accountId, DateTime? from, DateTime? to, Guid currentUserId);
    }
}
