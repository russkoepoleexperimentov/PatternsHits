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
    public class CreditsService : ICreditService
    {
        private readonly CreditDbContext _context;
        private readonly IMapper _mapper;
        private readonly IRequestClient<DepositFundsCommand> _depositClient;
        private readonly IValidator<CreateCreditRequest> _createCreditValidator;
        private readonly IValidator<ApproveCreditRequest> _approveValidator;
        private readonly IValidator<RejectCreditRequest> _rejectValidator;

        public CreditsService(
            CreditDbContext context,
            IMapper mapper,
            IRequestClient<DepositFundsCommand> depositClient,
            IValidator<CreateCreditRequest> createCreditValidator,
            IValidator<ApproveCreditRequest> approveValidator,
            IValidator<RejectCreditRequest> rejectValidator)
        {
            _context = context;
            _mapper = mapper;
            _depositClient = depositClient;
            _createCreditValidator = createCreditValidator;
            _approveValidator = approveValidator;
            _rejectValidator = rejectValidator;
        }

        public async Task<IEnumerable<CreditDto>> GetCreditsAsync(Guid? userId)
        {
            var query = _context.Credits.AsQueryable();
            if (userId.HasValue)
                query = query.Where(c => c.UserId == userId.Value);

            var credits = await query.ToListAsync();
            return _mapper.Map<IEnumerable<CreditDto>>(credits);
        }

        public async Task<CreditDto> GetCreditByIdAsync(Guid id)
        {
            var credit = await _context.Credits
                .Include(c => c.Tariff)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (credit == null)
                throw new KeyNotFoundException($"Credit {id} not found");

            return _mapper.Map<CreditDto>(credit);
        }

        public async Task<CreditDto> CreateCreditRequestAsync(CreateCreditRequest request, Guid currentUserId)
        {
            await _createCreditValidator.ValidateAndThrowAsync(request);

            var tariff = await _context.Tariffs.FindAsync(request.TariffId);
            if (tariff == null)
                throw new KeyNotFoundException($"Tariff {request.TariffId} not found");

            if (request.Amount > tariff.MaxAmount)
                throw new InvalidOperationException($"Amount exceeds tariff max {tariff.MaxAmount}");
            if (request.TermDays > tariff.MaxTermDays)
                throw new InvalidOperationException($"Term exceeds tariff max {tariff.MaxTermDays} days");

            var credit = _mapper.Map<Credit>(request);
            credit.UserId = currentUserId;
            credit.Status = CreditStatus.Pending;
            credit.RemainingDebt = 0;
            credit.AccountId = request.AccountId;
            credit.RejectionReason = "";
            credit.CreateDateTime = DateTime.UtcNow;
            credit.Currency = tariff.Currency;
            _context.Credits.Add(credit);
            await _context.SaveChangesAsync();

            return _mapper.Map<CreditDto>(credit);
        }

        public async Task<CreditDto> ApproveCreditAsync(Guid id, ApproveCreditRequest request, Guid employeeId)
        {
            await _approveValidator.ValidateAndThrowAsync(request);

            var credit = await _context.Credits
                .Include(c => c.Tariff)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (credit == null)
                throw new KeyNotFoundException($"Credit {id} not found");

            if (credit.Status != CreditStatus.Pending)
                throw new InvalidOperationException($"Credit cannot be approved in status {credit.Status}");

            var approvedAmount = request.ApprovedAmount ?? credit.Amount;
            if (approvedAmount > credit.Amount)
                throw new InvalidOperationException("Approved amount cannot exceed requested amount");
            if (approvedAmount <= 0)
                throw new InvalidOperationException("Approved amount must be positive");

            var correlationId = Guid.NewGuid();
            var response = await _depositClient.GetResponse<DepositFundsResponse>(
                new DepositFundsCommand(
                    credit.UserId,
                    credit.AccountId,
                    approvedAmount,
                    correlationId,
                    credit.Currency 
                ));

            if (!response.Message.Success)
            {
                credit.Status = CreditStatus.Rejected;
                credit.RejectionReason = response.Message.ErrorMessage ?? "Failed to deposit funds";
                credit.ApprovedBy = employeeId;
                credit.ApprovedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            else
            {
                credit.Status = CreditStatus.Approved;
                credit.ApprovedAmount = approvedAmount;
                credit.ApprovedBy = employeeId;
                credit.ApprovedAt = DateTime.UtcNow;
                credit.RemainingDebt = approvedAmount;

                decimal firstAmount = Math.Round(approvedAmount / credit.TermDays, 2);
                var firstPayment = new Payment
                {
                    CreditId = credit.Id,
                    Amount = firstAmount,
                    DueDate = credit.ApprovedAt.Value.AddHours(1),
                    Status = PaymentStatus.Pending,
                    CreateDateTime = DateTime.UtcNow,
                    FailureReason = string.Empty
                };
                _context.Payments.Add(firstPayment);

                await _context.SaveChangesAsync();
            }

            return _mapper.Map<CreditDto>(credit);
        }

        public async Task<CreditDto> RejectCreditAsync(Guid id, RejectCreditRequest request, Guid employeeId)
        {
            await _rejectValidator.ValidateAndThrowAsync(request);

            var credit = await _context.Credits.FindAsync(id);
            if (credit == null)
                throw new KeyNotFoundException($"Credit {id} not found");

            if (credit.Status != CreditStatus.Pending)
                throw new InvalidOperationException($"Credit cannot be rejected in status {credit.Status}");

            credit.Status = CreditStatus.Rejected;
            credit.RejectionReason = request.Reason;
            credit.ApprovedBy = employeeId;
            credit.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<CreditDto>(credit);
        }
    }
}