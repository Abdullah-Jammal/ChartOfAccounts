using Finance.Application.Features.Transactions.Common;
using FluentValidation;

namespace Finance.Application.Features.Transactions.CreateTransferTransaction;

public class CreateTransferTransactionCommandValidator : AbstractValidator<CreateTransferTransactionCommand>
{
    public CreateTransferTransactionCommandValidator()
    {
        this.AddCommonRules(
            command => command.Amount,
            command => command.Date,
            command => command.Description,
            command => command.ReferenceId);

        RuleFor(command => command.SourceAccountId)
            .GreaterThan(0);

        RuleFor(command => command.DestinationAccountId)
            .GreaterThan(0);

        RuleFor(command => command)
            .Must(command => command.SourceAccountId != command.DestinationAccountId)
            .WithMessage("Source and destination accounts must be different.");
    }
}
