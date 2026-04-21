using Finance.Domain.Accounting;
using FluentValidation;

namespace Finance.Application.Features.Journals.CreateJournal;

public class CreateJournalCommandValidator : AbstractValidator<CreateJournalCommand>
{
    public CreateJournalCommandValidator()
    {
        RuleFor(command => command.Date)
            .Must(date => date != default)
            .WithMessage("Journal date is required.");

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(Journal.MaxDescriptionLength);

        RuleFor(command => command.ReferenceNumber)
            .MaximumLength(Journal.MaxReferenceNumberLength)
            .When(command => !string.IsNullOrWhiteSpace(command.ReferenceNumber));
    }
}
