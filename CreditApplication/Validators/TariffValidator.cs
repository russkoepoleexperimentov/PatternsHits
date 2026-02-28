using CreditApplication.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditApplication.Validators
{
    public class TariffValidator : AbstractValidator<CreateTariffRequest>
    {
        public TariffValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(255).WithMessage("Name must not exceed 255 characters.");

            RuleFor(x => x.InterestRate)
                .NotEmpty().WithMessage("Interest rate is required.")
                .InclusiveBetween(0.01m, 100.0m).WithMessage("Interest rate must be between 0.01 and 100.0.");

            RuleFor(x => x.MaxAmount)
                .NotEmpty().WithMessage("Max amount is required.")
                .GreaterThan(0).WithMessage("Max amount must be greater than 0.");

            RuleFor(x => x.MaxTermDays)
                .NotEmpty().WithMessage("Max term days is required.")
                .GreaterThan(0).WithMessage("Max term days must be greater than 0.");
        }
    }
}
