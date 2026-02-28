using CreditApplication.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditApplication.Validators
{
    public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
    {
        public CreatePaymentRequestValidator()
        {
            RuleFor(x => x.Amount)
                .NotEmpty().WithMessage("Amount is required.")
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");

            When(x => x.PaymentDate.HasValue, () =>
            {
                RuleFor(x => x.PaymentDate.Value)
                    .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Payment date cannot be in the future.");
            });
        }
    }

    public class UpdatePaymentStatusRequestValidator : AbstractValidator<UpdatePaymentStatusRequest>
    {
        public UpdatePaymentStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid payment status.");
        }
    }
}
