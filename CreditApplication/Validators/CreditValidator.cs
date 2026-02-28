using CreditApplication.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditApplication.Validators
{
    public class CreateCreditRequestValidator : AbstractValidator<CreateCreditRequest>
    {
        public CreateCreditRequestValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty().WithMessage("Account ID is required.");

            RuleFor(x => x.TariffId)
                .NotEmpty().WithMessage("Tariff ID is required.");

            RuleFor(x => x.Amount)
                .NotEmpty().WithMessage("Amount is required.")
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");

            RuleFor(x => x.TermDays)
                .NotEmpty().WithMessage("Term days is required.")
                .GreaterThan(0).WithMessage("Term days must be greater than 0.");
        }
    }

    public class ApproveCreditRequestValidator : AbstractValidator<ApproveCreditRequest>
    {
        public ApproveCreditRequestValidator()
        {
            When(x => x.ApprovedAmount.HasValue, () =>
            {
                RuleFor(x => x.ApprovedAmount.Value)
                    .GreaterThan(0).WithMessage("Approved amount must be greater than 0.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Comment), () =>
            {
                RuleFor(x => x.Comment)
                    .MaximumLength(500).WithMessage("Comment must not exceed 500 characters.");
            });
        }
    }

    public class RejectCreditRequestValidator : AbstractValidator<RejectCreditRequest>
    {
        public RejectCreditRequestValidator()
        {
            When(x => !string.IsNullOrWhiteSpace(x.Reason), () =>
            {
                RuleFor(x => x.Reason)
                    .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
            });
        }
    }
}
