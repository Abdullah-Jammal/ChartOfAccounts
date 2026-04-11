using FluentValidation;

namespace Finance.Application.Features.Accounts.DeleteAccount;

public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(command => command.Code)
            .GreaterThan(0);
    }
}
