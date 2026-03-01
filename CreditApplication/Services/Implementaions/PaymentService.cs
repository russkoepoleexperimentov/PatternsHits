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
        private readonly ILogger<PaymentService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IValidator<CreatePaymentRequest> _createPaymentValidator;
        private readonly IValidator<UpdatePaymentStatusRequest> _updateStatusValidator;

        public PaymentService(
            CreditDbContext context,
            IMapper mapper,
            ILogger<PaymentService> logger,
            IPublishEndpoint publishEndpoint,
            IValidator<CreatePaymentRequest> createPaymentValidator,
            IValidator<UpdatePaymentStatusRequest> updateStatusValidator)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _createPaymentValidator = createPaymentValidator;
            _updateStatusValidator = updateStatusValidator;
        }

        public async Task<ProcessExternalPaymentResponse> ProcessExternalPaymentAsync(ProcessExternalPaymentCommand command)
        {
            try
            {
                var credit = await _context.Credits.FindAsync(command.CreditId);
                if (credit == null)
                    return new ProcessExternalPaymentResponse(false, $"Credit {command.CreditId} not found", null);

                if (credit.Status != CreditStatus.Approved && credit.Status != CreditStatus.Closed)
                    return new ProcessExternalPaymentResponse(false, $"Payments not allowed for credit in status {credit.Status}", null);
                if (credit.RemainingDebt <= 0)
                    return new ProcessExternalPaymentResponse(false, "No remaining debt", null);
                if (command.Amount > credit.RemainingDebt)
                    return new ProcessExternalPaymentResponse(false, $"Amount exceeds remaining debt {credit.RemainingDebt}", null);

                var payment = new Payment
                {
                    CreditId = command.CreditId,
                    Amount = command.Amount,
                    Status = PaymentStatus.Processed,
                    CreateDateTime = command.PaymentDate,
                    ProcessedAt = DateTime.UtcNow,
                    FailureReason = ""
                    
                };
                _context.Payments.Add(payment);

                credit.RemainingDebt -= command.Amount;
                if (credit.RemainingDebt == 0)
                {
                    credit.Status = CreditStatus.Closed;
                    credit.ClosedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                await _publishEndpoint.Publish(new PaymentProcessedEvent(
                    payment.Id,
                    command.CreditId,
                    command.Amount,
                    true,
                    null,
                    payment.ProcessedAt.Value
                ));

                _logger.LogInformation("External payment {PaymentId} processed for credit {CreditId}", payment.Id, command.CreditId);
                return new ProcessExternalPaymentResponse(true, "OK", payment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "External payment processing failed for credit {CreditId}", command.CreditId);
                return new ProcessExternalPaymentResponse(false, ex.Message, null);
            }
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