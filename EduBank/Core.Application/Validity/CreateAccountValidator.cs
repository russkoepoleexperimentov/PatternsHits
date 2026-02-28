using Core.Application.Dtos;
using FluentValidation;

namespace Core.Application.Validity
{
    public class CreateAccountValidator : AbstractValidator<CreateAccountDto>
    {
        public CreateAccountValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Currency).NotEmpty().Length(3);
            RuleFor(x => x.InitialBalance).GreaterThanOrEqualTo(0);
        }
    }
}
