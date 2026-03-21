using AutoMapper;
using Common.Contracts.AuthServiceContracts;
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
            var supportedCurrencies = new[] { "RUB", "PLN", "ILS" };
            if (!supportedCurrencies.Contains(dto.Currency))
                throw new InvalidOperationException($"Currency {dto.Currency} is not supported");

            var account = new Account
            {
                UserId = currentUserId,
                Balance = dto.InitialBalance,
                Currency = dto.Currency,
                ClosedAt = null
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return _mapper.Map<AccountDto>(account);
        }
        public async Task CloseAccountAsync(Guid id, Guid? currentUserId)
        {
            var account = await GetAccountFromDbAsync(id, currentUserId);
            account.ClosedAt = DateTime.UtcNow;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task<AccountDto> GetAccountByIdAsync(Guid id, Guid? currentUserId)
        {
            var account = await GetAccountFromDbAsync(id, currentUserId);
            return _mapper.Map<AccountDto>(account);
        }

        public async Task<List<AccountDto>> GetAccountsAsync(Guid? userId, Guid? currentUserId)
        {
            var accounts = await _context.Accounts.Where(x => x.UserId == userId).ToListAsync();
            return accounts.Select(_mapper.Map<AccountDto>).ToList();
        }

        public async Task<List<TransactionDto>> GetAccountTransactionsAsync(Guid accountId, DateTime? from, DateTime? to, Guid? currentUserId)
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

        public async Task<List<AccountTransactionDto>> GetAccountTransactionsForDisplayAsync(Guid accountId, DateTime? from, DateTime? to, Guid? currentUserId)
        {
            var account = await GetAccountFromDbAsync(accountId, currentUserId);

            var query = _context.Transactions
                .Where(t => (t.SourceId == accountId && t.SourceType == TransactionObjectType.Account) ||
                            (t.TargetId == accountId && t.TargetType == TransactionObjectType.Account));

            if (from.HasValue)
                query = query.Where(t => t.CreateDateTime >= from.Value);
            if (to.HasValue)
                query = query.Where(t => t.CreateDateTime <= to.Value);

            var transactions = await query
                .OrderByDescending(t => t.CreateDateTime)
                .ToListAsync();

            var dtos = new List<AccountTransactionDto>(transactions.Count);

            for (int i = 0; i < transactions.Count; i++)
            {
                var t = transactions[i];
                AccountTransactionDto dto = CreateTransactionDto(accountId, account, t);
                dtos[i] = dto;
            }

            return dtos;
        }

        public AccountTransactionDto CreateTransactionDto(Guid accountId, Account account, Transaction t)
        {
            var dto = _mapper.Map<AccountTransactionDto>(t);

            if (t.SourceId == accountId && t.SourceType == TransactionObjectType.Account)
            {
                dto.Amount = -t.Amount;
                dto.Currency = t.FromCurrency ?? account.Currency;
            }
            else if (t.TargetId == accountId && t.TargetType == TransactionObjectType.Account)
            {
                dto.Amount = t.ConvertedAmount ?? t.Amount;
                dto.Currency = t.ToCurrency ?? account.Currency;
            }
            else if (t.SourceType == TransactionObjectType.RealWorld)
            {
                dto.Amount = t.Amount;
                dto.Currency = t.ToCurrency ?? account.Currency;
            }
            else if (t.TargetType == TransactionObjectType.RealWorld)
            {
                dto.Amount = -t.Amount;
                dto.Currency = t.FromCurrency ?? account.Currency;
            }
            else
            {
                dto.Amount = 0;
                dto.Currency = account.Currency;
            }

            return dto;
        }

        public async Task<List<AccountDto>> GetAllAccountsAsync()
        {
            var accounts = await _context.Accounts.Where(x => !x.IsMaster).ToListAsync();
            return accounts.Select(_mapper.Map<AccountDto>).ToList();
        }

        public async Task<AccountDto> GetMasterAccountAsync()
        {
            var master = await _context.Accounts.FirstOrDefaultAsync(a => a.IsMaster);
            if (master == null)
                throw new NotFoundException("Master account not found");
            return _mapper.Map<AccountDto>(master);
        }

        public async Task<BlockUserAccountsResponse> BlockAccountAsync(BlockUserAccountsCommand cmd)
        {
            try
            {
                var accounts = await _context.Accounts.Where(x => x.UserId == cmd.UserId).ToListAsync();
                foreach (var account in accounts)
                {
                    account.IsDeleted = true;
                    _context.Accounts.Update(account);
                }
                await _context.SaveChangesAsync();
                return new(true, null);
            }
            catch (Exception ex) { 
                return new(false, ex.Message);
            }
        }

        public async Task<UnblockUserAccountsResponse> UnblockAccountAsync(UnblockUserAccountsCommand cmd)
        {
            try
            {
                var accounts = await _context.Accounts.Where(x => x.UserId == cmd.UserId).ToListAsync();
                foreach (var account in accounts)
                {
                    account.IsDeleted = false;
                    _context.Accounts.Update(account);
                }
                await _context.SaveChangesAsync();
                return new(true, null);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }
    }
}
