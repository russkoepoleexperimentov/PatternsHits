using Core.Application.Dtos;
using Core.Domain;
using FluentValidation;

namespace Core.Application.Validity
{
    public class CreateTransactionValidator : AbstractValidator<CreateTransactionDto>
    {
        public CreateTransactionValidator()
        {
            RuleFor(x => x.SourceType)
            .NotEqual(TransactionObjectType.Credit)
            .WithMessage("Source type cannot be Credit.");

            // Правило: SourceType и TargetType не могут одновременно быть RealWorld.
            RuleFor(x => x)
                .Must(x => !(x.SourceType == TransactionObjectType.RealWorld && x.TargetType == TransactionObjectType.RealWorld))
                .WithMessage("Source and Target cannot both be RealWorld.");

            // SourceId не null, если SourceType != RealWorld
            RuleFor(x => x.SourceId)
                .NotNull()
                .When(x => x.SourceType != TransactionObjectType.RealWorld)
                .WithMessage("SourceId is required when SourceType is not RealWorld.");

            // TargetId не null, если TargetType != RealWorld
            RuleFor(x => x.TargetId)
                .NotNull()
                .When(x => x.TargetType != TransactionObjectType.RealWorld)
                .WithMessage("TargetId is required when TargetType is not RealWorld.");


            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description must be less than 500 chars.");
        }
    }
}
