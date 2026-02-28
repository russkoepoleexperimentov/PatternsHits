using AutoMapper;
using Common.Exceptions;
using Core.Application.Dtos;
using Core.Application.Services.Interfaces;
using Core.Domain;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Application.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly IValidator<CreateAccountDto> _createAccountValidator;
        private readonly CoreDbContext _context;

        public AccountService(
            IMapper mapper,
            IValidator<CreateAccountDto> createAccountValidator,
            CoreDbContext context)
        {
            _mapper = mapper;
            _createAccountValidator = createAccountValidator;
            _context = context;
        }

        public Task CloseAccountAsync(Guid id, Guid currentUserId)
        {
            throw new NotImplementedException();
        }

        public Task<AccountDto> CreateAccountAsync(CreateAccountDto dto, Guid currentUserId)
        {
            throw new NotImplementedException();
        }

        public Task<AccountDto> GetAccountByIdAsync(Guid id, Guid currentUserId)
        {
            throw new NotImplementedException();
        }

        public Task<List<AccountDto>> GetAccountsAsync(Guid? userId, Guid currentUserId)
        {
            throw new NotImplementedException();
        }

        public Task<List<TransactionDto>> GetAccountTransactionsAsync(Guid accountId, DateTime? from, DateTime? to, Guid currentUserId)
        {
            throw new NotImplementedException();
        }
    }
}
