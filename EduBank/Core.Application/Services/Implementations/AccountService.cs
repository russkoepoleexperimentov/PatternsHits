using AutoMapper;
using Common.Exceptions;
using Core.Application.Dtos;
using Core.Application.Services.Interfaces;
using Core.Domain;
using Core.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Core.Application.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly CoreDbContext _context;

        public AccountService(
            IMapper mapper,
            CoreDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<AccountDto> CreateAccountAsync(Guid currentUserId, CreateAccountDto dto)
        {
            var account = new Account()
            {
                UserId = currentUserId,
                Balance = dto.InitialBalance,
                ClosedAt = null
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return _mapper.Map<AccountDto>(account);    
        }

        public async Task CloseAccountAsync(Guid id, Guid currentUserId)
        {
            var account = await GetAccountFromDbAsync(id, currentUserId);
            account.IsDeleted = true;
            account.ClosedAt = DateTime.UtcNow;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task<AccountDto> GetAccountByIdAsync(Guid id, Guid currentUserId)
        {
            var account = await GetAccountFromDbAsync(id, currentUserId);
            return _mapper.Map<AccountDto>(account);
        }

        public async Task<List<AccountDto>> GetAccountsAsync(Guid? userId, Guid currentUserId)
        {
            var accounts = await _context.Accounts.Where(x => x.UserId == userId).ToListAsync();
            return accounts.Select(_mapper.Map<AccountDto>).ToList();
        }

        public async Task<List<TransactionDto>> GetAccountTransactionsAsync(Guid accountId, DateTime? from, DateTime? to, Guid currentUserId)
        {
            var account = await GetAccountFromDbAsync(accountId, currentUserId);
            var transactions = await _context.Transactions.Where(x => 
                (x.SourceId == accountId && x.SourceType == TransactionObjectType.Account) ||
                (x.TargetId == accountId && x.TargetType == TransactionObjectType.Account)
            ).ToListAsync();
            return transactions.Select(_mapper.Map<TransactionDto>).ToList();
        }

        public async Task<Account> GetAccountFromDbAsync(Guid accountId, Guid? ownerId)
        {
            var account =  await _context.FindAsync<Account>(accountId);

            if(account == null)
            {
                throw new NotFoundException(nameof(Account));
            }

            if(ownerId.HasValue && account.UserId != ownerId.Value)
            {
                throw new ForbiddenException();
            }

            return account;
        }
    }
}
