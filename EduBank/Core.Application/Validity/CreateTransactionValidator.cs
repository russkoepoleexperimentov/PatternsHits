using Core.Application.Dtos;
using FluentValidation;

namespace Core.Application.Validity
{
    public class CreateTransactionValidator : AbstractValidator<CreateTransactionDto>
    {
        public CreateTransactionValidator()
        {
            RuleFor(x => x.AccountId).NotEmpty();
            RuleFor(x => x.Type).Must(t => t == "deposit" || t == "withdraw")
                .WithMessage("Type must be 'deposit' or 'withdraw'");
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Description).MaximumLength(500);
        }
    }
}
