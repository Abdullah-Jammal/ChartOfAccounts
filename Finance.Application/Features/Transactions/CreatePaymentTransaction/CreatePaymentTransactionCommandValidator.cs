using Finance.Application.Features.Transactions.Common;
using FluentValidation;

namespace Finance.Application.Features.Transactions.CreatePaymentTransaction;

public class CreatePaymentTransactionCommandValidator : AbstractValidator<CreatePaymentTransactionCommand>
{
    public CreatePaymentTransactionCommandValidator()
    {
        this.AddCommonRules(
            command => command.Amount,
            command => command.Date,
            command => command.Description,
            command => command.ReferenceId);

        RuleFor(command => command.CashAccountId)
            .GreaterThan(0);

        RuleFor(command => command.RevenueAccountId)
            .GreaterThan(0);

        RuleFor(command => command)
            .Must(command => command.CashAccountId != command.RevenueAccountId)
            .WithMessage("Cash and revenue accounts must be different.");
    }
}
