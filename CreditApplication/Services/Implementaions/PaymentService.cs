using AutoMapper;
using Common.Contracts;
using Common.Enums;
using CreditApplication.Dtos;
using CreditApplication.Services.Interfaces;
using CreditApplication.Validators;
using CreditDomain.Entities;
using CreditInfrastructure;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CreditService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly CreditDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IValidator<CreatePaymentRequest> _createPaymentValidator;
        private readonly IValidator<UpdatePaymentStatusRequest> _updateStatusValidator;
        private readonly ICurrencyRateService _currencyRateService;

        public PaymentService(
            CreditDbContext context,
            IMapper mapper,
            IPublishEndpoint publishEndpoint,
            IValidator<CreatePaymentRequest> createPaymentValidator,
            IValidator<UpdatePaymentStatusRequest> updateStatusValidator,
            ICurrencyRateService currencyRateService)
        {
            _context = context;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
            _createPaymentValidator = createPaymentValidator;
            _updateStatusValidator = updateStatusValidator;
            _currencyRateService = currencyRateService;
        }

        public async Task<ProcessExternalPaymentResponse> ProcessExternalPaymentAsync(ProcessExternalPaymentCommand command)
        {
            try
            {
                var credit = await _context.Credits.FindAsync(command.CreditId);
                if (credit == null)
                    return new ProcessExternalPaymentResponse(false, $"Credit {command.CreditId} not found", null);

                if (credit.Status != CreditStatus.Approved)
                    return new ProcessExternalPaymentResponse(false, $"Payments not allowed for credit in status {credit.Status}", null);
                if (credit.RemainingDebt <= 0)
                    return new ProcessExternalPaymentResponse(false, "No remaining debt", null);

                var paymentCurrency = command.Currency;
                var creditCurrency = credit.Currency;

                decimal amountInCreditCurrency;
                decimal? exchangeRate = null;
                decimal? originalAmount = null;

                if (paymentCurrency == creditCurrency)
                {
                    amountInCreditCurrency = command.Amount;
                }
                else
                {
                    exchangeRate = await _currencyRateService.GetExchangeRateAsync(paymentCurrency, creditCurrency);
                    amountInCreditCurrency = command.Amount * exchangeRate.Value;
                    originalAmount = command.Amount;
                }

                if (amountInCreditCurrency >= credit.RemainingDebt)
                {
                    if (amountInCreditCurrency > credit.RemainingDebt + 0.01m)
                        return new ProcessExternalPaymentResponse(false, $"Amount after conversion exceeds remaining debt {credit.RemainingDebt}", null);


                    var pending = await _context.Payments
                        .Where(p => p.CreditId == credit.Id && p.Status == PaymentStatus.Pending)
                        .ToListAsync();
                    _context.Payments.RemoveRange(pending);

                    var finalPayment = new Payment
                    {
                        CreditId = credit.Id,
                        Amount = credit.RemainingDebt, 
                        OriginalAmount = originalAmount,
                        OriginalCurrency = originalAmount.HasValue ? paymentCurrency : null,
                        ExchangeRate = exchangeRate,
                        DueDate = DateTime.UtcNow,
                        Status = PaymentStatus.Processed,
                        ProcessedAt = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    };
                    _context.Payments.Add(finalPayment);

                    credit.RemainingDebt = 0;
                    credit.Status = CreditStatus.Closed;
                    credit.ClosedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    await _publishEndpoint.Publish(new PaymentProcessedEvent(
                        finalPayment.Id,
                        command.CreditId,
                        command.Amount,
                        true,
                        null,
                        finalPayment.ProcessedAt.Value
                    ));

                    return new ProcessExternalPaymentResponse(true, "Credit fully repaid", finalPayment.Id);
                }

                var currentPayment = await _context.Payments
                    .Where(p => p.CreditId == credit.Id && p.Status == PaymentStatus.Pending)
                    .OrderBy(p => p.DueDate)
                    .FirstOrDefaultAsync();

                if (currentPayment == null)
                    return new ProcessExternalPaymentResponse(false, "No pending payment found", null);

                if (Math.Abs(amountInCreditCurrency - currentPayment.Amount) > 0.01m)
                    return new ProcessExternalPaymentResponse(false,
                        $"Amount after conversion ({amountInCreditCurrency:F2}) must equal current payment amount {currentPayment.Amount:F2} or full debt {credit.RemainingDebt:F2}", null);

                currentPayment.Status = PaymentStatus.Processed;
                currentPayment.ProcessedAt = DateTime.UtcNow;
                currentPayment.OriginalAmount = originalAmount;
                currentPayment.OriginalCurrency = originalAmount.HasValue ? paymentCurrency : null;
                currentPayment.ExchangeRate = exchangeRate;

                credit.RemainingDebt -= amountInCreditCurrency;

                var createdCount = await _context.Payments
                    .CountAsync(p => p.CreditId == credit.Id && (p.Status == PaymentStatus.Processed || p.Status == PaymentStatus.Overdue));

                if (createdCount < credit.TermDays)
                {
                    int remainingDays = credit.TermDays - createdCount;
                    decimal nextAmount = (remainingDays == 1)
                        ? credit.RemainingDebt
                        : Math.Round(credit.RemainingDebt / remainingDays, 2);

                    var nextPayment = new Payment
                    {
                        CreditId = credit.Id,
                        Amount = nextAmount,
                        DueDate = DateTime.UtcNow.AddHours(1),
                        Status = PaymentStatus.Pending,
                        CreateDateTime = DateTime.UtcNow
                    };
                    _context.Payments.Add(nextPayment);
                }
                else if (credit.RemainingDebt > 0)
                {
                    var lastPayment = new Payment
                    {
                        CreditId = credit.Id,
                        Amount = credit.RemainingDebt,
                        DueDate = DateTime.UtcNow.AddHours(1),
                        Status = PaymentStatus.Pending,
                        CreateDateTime = DateTime.UtcNow
                    };
                    _context.Payments.Add(lastPayment);
                }

                await _context.SaveChangesAsync();

                await _publishEndpoint.Publish(new PaymentProcessedEvent(
                    currentPayment.Id,
                    command.CreditId,
                    command.Amount,
                    true,
                    null,
                    currentPayment.ProcessedAt.Value
                ));

                return new ProcessExternalPaymentResponse(true, "OK", currentPayment.Id);
            }
            catch (Exception ex)
            {
                return new ProcessExternalPaymentResponse(false, ex.Message, null);
            }
        }

        public async Task<IEnumerable<PaymentDto>> GetOverdueByCreditIdAsync(Guid creditId)
        {
            var overdue = await _context.Payments
                .Where(p => p.CreditId == creditId && p.Status == PaymentStatus.Overdue)
                .ToListAsync();
            return _mapper.Map<IEnumerable<PaymentDto>>(overdue);
        }

        public async Task<IEnumerable<PaymentDto>> GetOverdueByUserIdAsync(Guid userId)
        {
            var overdue = await _context.Payments
                .Include(p => p.Credit)
                .Where(p => p.Credit.UserId == userId && p.Status == PaymentStatus.Overdue)
                .ToListAsync();
            return _mapper.Map<IEnumerable<PaymentDto>>(overdue);
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByCreditIdAsync(Guid creditId)
        {
            var payments = await _context.Payments
                .Where(p => p.CreditId == creditId)
                .OrderByDescending(p => p.CreateDateTime)
                .ToListAsync();
            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }

        public async Task<PaymentDto> GetPaymentByIdAsync(Guid id)
        {
            var payment = await _context.Payments
                .Include(p => p.Credit)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment {id} not found");
            return _mapper.Map<PaymentDto>(payment);
        }
    }
}