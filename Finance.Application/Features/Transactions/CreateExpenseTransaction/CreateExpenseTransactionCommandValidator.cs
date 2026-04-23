using Finance.Application.Features.Transactions.Common;
using FluentValidation;

namespace Finance.Application.Features.Transactions.CreateExpenseTransaction;

public class CreateExpenseTransactionCommandValidator : AbstractValidator<CreateExpenseTransactionCommand>
{
    public CreateExpenseTransactionCommandValidator()
    {
        this.AddCommonRules(
            command => command.Amount,
            command => command.Date,
            command => command.Description,
            command => command.ReferenceId);

        RuleFor(command => command.ExpenseAccountId)
            .GreaterThan(0);

        RuleFor(command => command.CashAccountId)
            .GreaterThan(0);

        RuleFor(command => command)
            .Must(command => command.ExpenseAccountId != command.CashAccountId)
            .WithMessage("Expense and cash accounts must be different.");
    }
}
