using Core.Application.Dtos;
using FluentValidation;

namespace Core.Application.Validity
{
    public class CreateCommentValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentValidator()
        {
            RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
        }
    }
}
