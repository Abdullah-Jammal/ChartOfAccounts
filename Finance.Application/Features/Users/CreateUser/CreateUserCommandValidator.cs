using FluentValidation;

namespace Finance.Application.Features.Users.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(x => x.FullName)
            .MinimumLength(3)
            .When(x => !string.IsNullOrWhiteSpace(x.FullName));

        RuleForEach(x => x.Roles)
            .NotEmpty()
            .When(x => x.Roles is not null);
    }
}
