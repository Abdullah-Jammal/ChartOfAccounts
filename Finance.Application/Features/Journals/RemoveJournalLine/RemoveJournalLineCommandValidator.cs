using FluentValidation;

namespace Finance.Application.Features.Journals.RemoveJournalLine;

public class RemoveJournalLineCommandValidator : AbstractValidator<RemoveJournalLineCommand>
{
    public RemoveJournalLineCommandValidator()
    {
        RuleFor(command => command.JournalId)
            .GreaterThan(0);

        RuleFor(command => command.LineId)
            .GreaterThan(0);
    }
}
