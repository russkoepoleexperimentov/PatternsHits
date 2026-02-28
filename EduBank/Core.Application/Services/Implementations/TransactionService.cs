using AutoMapper;
using Common.Contracts;
using Common.Exceptions;
using Core.Application.Dtos;
using Core.Application.Services.Interfaces;
using Core.Domain;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Application.Services.Implementations
{
    public class TransactionService : ITransactionService
    {

        private readonly CoreDbContext _context;
        private readonly IAccountService _accountService;
        private readonly IValidator<CreateTransactionDto> _createValidator;
        private readonly IMapper _mapper;

        public TransactionService(CoreDbContext context, IAccountService accountService, IValidator<CreateTransactionDto> createValidator, IMapper mapper)
        {
            _context = context;
            _accountService = accountService;
            _createValidator = createValidator;
            _mapper = mapper;
        }

        public async Task<TransactionDto> InitializeTransactionAsync(CreateTransactionDto dto, Guid currentUserId)
        {
            _createValidator.ValidateAndThrow(dto);
            var transaction = _mapper.Map<Transaction>(dto);

            if(transaction.SourceType == TransactionObjectType.Account)
            {
                // счёт-источник (привязанный к юзеру)
                var sourceAcc = await _accountService.GetAccountFromDbAsync(transaction.SourceId!.Value, currentUserId);

                if (transaction.TargetType == TransactionObjectType.Account) // перевод со счёта на счёт
                {
                    // счёт-цель (любой!!)
                    var targetAcc = await _accountService.GetAccountFromDbAsync(transaction.TargetId!.Value, null);
                    ApplyTransfer(transaction, sourceAcc, targetAcc);
                }
                else if(transaction.TargetType == TransactionObjectType.RealWorld) // снятие денег
                {
                    ApplyWithdraw(transaction, sourceAcc);
                }
            }
            else if (transaction.SourceType == TransactionObjectType.RealWorld)
            {
                // счёт-цель (привязанный к юзеру)
                if (transaction.TargetType == TransactionObjectType.Account) // пополнение
                {
                    // ensure existance of target account
                    var targetAcc = await _accountService.GetAccountFromDbAsync(transaction.TargetId!.Value, currentUserId);
                    ApplyDeposit(transaction, targetAcc);
                }
            }

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return _mapper.Map<TransactionDto>(transaction);
        }

        public async Task<TransactionDto> GetTransactionByIdAsync(Guid id, Guid currentUserId)
        {
            throw new NotImplementedException();
        }

        public async Task<TransactionDto> ResolveTransactionAsync(Guid id, TransactionStatus status, string message)
        {
            throw new NotImplementedException();
        }

        private void ApplyTransfer(Transaction transaction, Account source, Account target)
        {
            if(source.Balance < transaction.Amount)
            {
                transaction.Status = TransactionStatus.Failed;
                transaction.ResolvedAt = DateTime.UtcNow;
                transaction.ResolutionMessage = "На счёте недостаточно денег";
                return;
            }

            source.Balance -= transaction.Amount;
            target.Balance += transaction.Amount;
            transaction.Status = TransactionStatus.Completed;
            transaction.ResolvedAt = DateTime.UtcNow;
            transaction.ResolutionMessage = "Перевод выполнен";
        }

        private void ApplyDeposit(Transaction transaction, Account target)
        {
            target.Balance += transaction.Amount;
            transaction.Status = TransactionStatus.Completed;
            transaction.ResolvedAt = DateTime.UtcNow;
            transaction.ResolutionMessage = "Пополнение счёта";
        }

        private void ApplyWithdraw(Transaction transaction, Account source)
        {
            source.Balance -= transaction.Amount;
            transaction.Status = TransactionStatus.Completed;
            transaction.ResolvedAt = DateTime.UtcNow;
            transaction.ResolutionMessage = "Снятие денег со счёта";
        }

        private async Task<Transaction> GetTransactionFromDbAsync(Guid transactionId)
        {
            var account = await _context.FindAsync<Transaction>(transactionId);

            if (account == null)
            {
                throw new NotFoundException(nameof(Transaction));
            }

            return account;
        }

        public async Task<DepositFundsResponse> ProcessDepositFund(DepositFundsCommand command)
        {
            try
            {
                // ensure existance of target account
                var targetAcc = await _accountService.GetAccountFromDbAsync(command.AccountId, command.UserId);
                var transaction = new Transaction()
                {
                    SourceId = command.CorrelationId,
                    SourceType = TransactionObjectType.Credit,
                    TargetId = targetAcc.Id,
                    TargetType = TransactionObjectType.Account,
                    Description = "Кредит",
                    Amount = command.Amount,
                    Status = TransactionStatus.Completed,
                };

                targetAcc.Balance += command.Amount;

                _context.Transactions.Add(transaction);
                _context.Accounts.Update(targetAcc);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) { 
                return new(false, ex.Message);
            }

            return new(true, null);
        }
    }
}
