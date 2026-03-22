using AutoMapper;
using Common.Contracts;
using Common.Enums;
using Common.Exceptions;
using Core.Application.Dtos;
using Core.Application.Services.Interfaces;
using Core.Domain;
using Core.Infrastructure;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Core.Application.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly CoreDbContext _context;
        private readonly IAccountService _accountService;
        private readonly IValidator<CreateTransactionDto> _createValidator;
        private readonly IRequestClient<ProcessExternalPaymentCommand> _paymentClient;
        private readonly IMapper _mapper;
        private readonly ICurrencyRateService _currencyRateService;
        private readonly IRequestClient<ProcessTransactionCommand> _transactionRequestClient;
        private readonly TransactionsWebSocketConnectionManager _transactionsWebSocketConnectionManager;

        public TransactionService(
            CoreDbContext context,
            IAccountService accountService,
            IValidator<CreateTransactionDto> createValidator,
            IMapper mapper,
            IRequestClient<ProcessExternalPaymentCommand> paymentClient,
            ICurrencyRateService currencyRateService,
            IRequestClient<ProcessTransactionCommand> transactionRequestClient,
            TransactionsWebSocketConnectionManager transactionsWebSocketConnectionManager)
        {
            _context = context;
            _accountService = accountService;
            _createValidator = createValidator;
            _mapper = mapper;
            _paymentClient = paymentClient;
            _currencyRateService = currencyRateService;
            _transactionRequestClient = transactionRequestClient;
            _transactionsWebSocketConnectionManager = transactionsWebSocketConnectionManager;
        }

        public async Task<TransactionDto> InitializeTransactionAsync(CreateTransactionDto dto, Guid currentUserId)
        {
            _createValidator.ValidateAndThrow(dto);

            var transactionId = Guid.NewGuid();
            var command = new ProcessTransactionCommand(transactionId, dto, currentUserId);

            var response = await _transactionRequestClient.GetResponse<ProcessTransactionResponse>(command);

            if (!response.Message.Success)
                throw new BadRequestException(response.Message.ErrorMessage);

            return response.Message.Transaction!;
        }

        public async Task<TransactionDto> ExecuteTransactionAsync(CreateTransactionDto dto, Guid userId, Guid transactionId)
        {
            var existing = await GetTransactionByIdIfExistsAsync(transactionId);
            if (existing != null)
                return existing;

            var transaction = _mapper.Map<Transaction>(dto);
            transaction.Id = transactionId;

            if (transaction.SourceType == TransactionObjectType.Account)
            {
                var sourceAcc = await _accountService.GetAccountFromDbAsync(transaction.SourceId!.Value, userId);

                if (transaction.TargetType == TransactionObjectType.Account)
                {
                    var targetAcc = await _accountService.GetAccountFromDbAsync(transaction.TargetId!.Value, null);

                    if (sourceAcc.Currency != targetAcc.Currency)
                    {
                        var rate = await _currencyRateService.GetExchangeRateAsync(sourceAcc.Currency, targetAcc.Currency);
                        var convertedAmount = transaction.Amount * rate;

                        transaction.ConvertedAmount = convertedAmount;
                        transaction.FromCurrency = sourceAcc.Currency;
                        transaction.ToCurrency = targetAcc.Currency;
                        transaction.ExchangeRate = rate;

                        ApplyTransferWithConversion(transaction, sourceAcc, targetAcc, convertedAmount);
                    }
                    else
                    {
                        ApplyTransfer(transaction, sourceAcc, targetAcc);
                    }
                }
                else if (transaction.TargetType == TransactionObjectType.RealWorld)
                {
                    ApplyWithdraw(transaction, sourceAcc);
                }
                else if (transaction.TargetType == TransactionObjectType.Credit)
                {
                    await ApplyCreditPayment(transaction, sourceAcc);
                }
                else throw new InvalidOperationException();
            }
            else if (transaction.SourceType == TransactionObjectType.RealWorld)
            {
                if (transaction.TargetType == TransactionObjectType.Account)
                {
                    var targetAcc = await _accountService.GetAccountFromDbAsync(transaction.TargetId!.Value, null);
                    ApplyDeposit(transaction, targetAcc);
                }
                else throw new InvalidOperationException();
            }
            else throw new InvalidOperationException();

            // notify
            var accounts = await _context.Accounts.Where(acc => transaction.SourceId == acc.Id || transaction.TargetId == acc.Id).ToListAsync();

            var transactionDto = _mapper.Map<TransactionDto>(transaction);

            foreach (var account in accounts) {
                var displayDto = _accountService.CreateTransactionDto(account.Id, account, transaction);
                await _transactionsWebSocketConnectionManager.NotifyAllInterested(transactionDto, displayDto, account);
            }
            
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transactionDto;
        }

        public async Task<TransactionDto?> GetTransactionByIdIfExistsAsync(Guid transactionId)
        {
            var transaction = await _context.Transactions.FindAsync(transactionId);
            return transaction == null ? null : _mapper.Map<TransactionDto>(transaction);
        }

        public async Task<DepositFundsResponse> ProcessDepositFund(DepositFundsCommand command)
        {
            try
            {
                var targetAcc = await _accountService.GetAccountFromDbAsync(command.AccountId, command.UserId);
                if (targetAcc == null)
                    return new DepositFundsResponse(false, "Target account not found");

                var master = await GetMasterAccountAsync();

                decimal amountInTargetCurrency;
                decimal amountFromMaster;
                decimal? exchangeRate1 = null;
                decimal? exchangeRate2 = null;

                if (command.Currency != targetAcc.Currency)
                {
                    exchangeRate1 = await _currencyRateService.GetExchangeRateAsync(command.Currency, targetAcc.Currency);
                    amountInTargetCurrency = command.Amount * exchangeRate1.Value;
                }
                else
                {
                    amountInTargetCurrency = command.Amount;
                }

                if (command.Currency != master.Currency)
                {
                    exchangeRate2 = await _currencyRateService.GetExchangeRateAsync(command.Currency, master.Currency);
                    amountFromMaster = command.Amount * exchangeRate2.Value;
                }
                else
                {
                    amountFromMaster = command.Amount;
                }

                if (master.Balance < amountFromMaster)
                    return new DepositFundsResponse(false, "Insufficient funds on master account");

                master.Balance -= amountFromMaster;
                targetAcc.Balance += amountInTargetCurrency;

                var transaction = new Transaction
                {
                    SourceId = master.Id,
                    SourceType = TransactionObjectType.Account,
                    TargetId = targetAcc.Id,
                    TargetType = TransactionObjectType.Account,
                    Description = "Выдача кредита",
                    Amount = amountInTargetCurrency,
                    Status = TransactionStatus.Completed,
                    ResolvedAt = DateTime.UtcNow,
                    ResolutionMessage = "Кредит выдан",
                    FromCurrency = master.Currency, 
                    ToCurrency = targetAcc.Currency, 
                    ConvertedAmount = amountFromMaster, 
                    ExchangeRate = exchangeRate1 
                };

                _context.Transactions.Add(transaction);
                _context.Accounts.Update(master);
                _context.Accounts.Update(targetAcc);
                await _context.SaveChangesAsync();

                return new DepositFundsResponse(true, null);
            }
            catch (Exception ex)
            {
                return new DepositFundsResponse(false, ex.Message);
            }
        }
        public async Task<TransactionDto> GetTransactionByIdAsync(Guid id, Guid currentUserId)
        {
            var transaction = await GetTransactionFromDbAsync(id);
            return _mapper.Map<TransactionDto>(transaction);
        }

        private async Task ApplyCreditPayment(Transaction transaction, Account source)
        {
            if (!EnsureCanInitialize(transaction, source)) return;
            if (!EnsureCanWithdraw(transaction, source)) return;

            var response = await _paymentClient.GetResponse<ProcessExternalPaymentResponse>(
                new ProcessExternalPaymentCommand(
                    transaction.TargetId!.Value,
                    transaction.Amount,
                    transaction.Id.ToString(),
                    DateTime.UtcNow,
                    source.Currency));

            if (!response.Message.Success)
            {
                transaction.Status = TransactionStatus.Failed;
                transaction.ResolutionMessage = response.Message.Message;
                transaction.ResolvedAt = DateTime.UtcNow;
            }
            else
            {
                var master = await _context.Accounts.FirstOrDefaultAsync(a => a.IsMaster);
                if (master == null)
                {
                    transaction.Status = TransactionStatus.Failed;
                    transaction.ResolutionMessage = "Master account not found";
                    transaction.ResolvedAt = DateTime.UtcNow;
                    return;
                }

                decimal amountInCreditCurrency = response.Message.AmountInCreditCurrency;

                decimal amountForMaster = amountInCreditCurrency;
                if (response.Message.CreditCurrency != master.Currency)
                {
                    var rate = await _currencyRateService.GetExchangeRateAsync(response.Message.CreditCurrency, master.Currency);
                    amountForMaster = amountInCreditCurrency * rate;
                }

                source.Balance -= transaction.Amount;
                master.Balance += amountForMaster;

                transaction.ConvertedAmount = response.Message.AmountInCreditCurrency;
                transaction.FromCurrency = source.Currency;
                transaction.ToCurrency = response.Message.CreditCurrency;
                transaction.ExchangeRate = response.Message.ExchangeRate;

                transaction.Status = TransactionStatus.Completed;
                transaction.ResolutionMessage = "Оплата кредита";
                transaction.ResolvedAt = DateTime.UtcNow;

                _context.Accounts.Update(source);
                _context.Accounts.Update(master);
            }
        }
        private void ApplyTransferWithConversion(Transaction transaction, Account source, Account target, decimal convertedAmount)
        {
            if (!EnsureCanInitialize(transaction, source)) return;
            if (!EnsureCanInitialize(transaction, target)) return;
            if (!EnsureCanWithdraw(transaction, source)) return;

            source.Balance -= transaction.Amount;
            target.Balance += convertedAmount;

            transaction.Status = TransactionStatus.Completed;
            transaction.ResolvedAt = DateTime.UtcNow;
            transaction.ResolutionMessage = $"Перевод с конвертацией {source.Currency} → {target.Currency}";
        }

        private void ApplyTransfer(Transaction transaction, Account source, Account target)
        {
            if (!EnsureCanInitialize(transaction, source)) return;
            if (!EnsureCanInitialize(transaction, target)) return;
            if (!EnsureCanWithdraw(transaction, source)) return;

            source.Balance -= transaction.Amount;
            target.Balance += transaction.Amount;
            transaction.Status = TransactionStatus.Completed;
            transaction.ResolvedAt = DateTime.UtcNow;
            transaction.ResolutionMessage = "Перевод выполнен";
        }

        private static bool EnsureCanInitialize(Transaction transaction, Account source)
        {
            if (source.IsDeleted || source.ClosedAt != null)
            {
                transaction.Status = TransactionStatus.Failed;
                transaction.ResolvedAt = DateTime.UtcNow;
                transaction.ResolutionMessage = "Счёт закрыт";
                return false;
            }
            return true;
        }

        private static bool EnsureCanWithdraw(Transaction transaction, Account source)
        {
            if (source.Balance < transaction.Amount)
            {
                transaction.Status = TransactionStatus.Failed;
                transaction.ResolvedAt = DateTime.UtcNow;
                transaction.ResolutionMessage = "На счёте недостаточно денег";
                return false;
            }
            return true;
        }

        private void ApplyDeposit(Transaction transaction, Account target)
        {
            if (!EnsureCanInitialize(transaction, target)) return;
            target.Balance += transaction.Amount;
            transaction.Status = TransactionStatus.Completed;
            transaction.ResolvedAt = DateTime.UtcNow;
            transaction.ResolutionMessage = "Пополнение счёта";
        }

        private void ApplyWithdraw(Transaction transaction, Account source)
        {
            if (!EnsureCanInitialize(transaction, source)) return;
            if (!EnsureCanWithdraw(transaction, source)) return;

            source.Balance -= transaction.Amount;
            transaction.Status = TransactionStatus.Completed;
            transaction.ResolvedAt = DateTime.UtcNow;
            transaction.ResolutionMessage = "Снятие денег со счёта";
        }

        private async Task<Account> GetMasterAccountAsync()
        {
            var master = await _context.Accounts.FirstOrDefaultAsync(a => a.IsMaster);
            if (master == null)
                throw new InvalidOperationException("Master account not found");
            return master;
        }

        private async Task<Transaction> GetTransactionFromDbAsync(Guid transactionId)
        {
            var transaction = await _context.FindAsync<Transaction>(transactionId);
            if (transaction == null)
                throw new NotFoundException(nameof(Transaction));
            return transaction;
        }
    }
}