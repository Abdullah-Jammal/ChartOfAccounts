using FluentValidation;

namespace Finance.Application.Features.Journals.AddJournalLine;

public class AddJournalLineCommandValidator : AbstractValidator<AddJournalLineCommand>
{
    public AddJournalLineCommandValidator()
    {
        RuleFor(command => command.JournalId)
            .GreaterThan(0);

        RuleFor(command => command.AccountId)
            .GreaterThan(0);

        RuleFor(command => command.Debit)
            .GreaterThanOrEqualTo(0)
            .Must(HaveSupportedScale)
            .WithMessage("Debit cannot have more than 2 decimal places.");

        RuleFor(command => command.Credit)
            .GreaterThanOrEqualTo(0)
            .Must(HaveSupportedScale)
            .WithMessage("Credit cannot have more than 2 decimal places.");

        RuleFor(command => command)
            .Must(command => command.Debit > 0m ^ command.Credit > 0m)
            .WithMessage("Each journal line must contain either debit or credit, but not both.");
    }

    private static bool HaveSupportedScale(decimal amount)
    {
        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero) == amount;
    }
}
