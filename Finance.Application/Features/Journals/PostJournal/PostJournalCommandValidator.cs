using FluentValidation;

namespace Finance.Application.Features.Journals.PostJournal;

public class PostJournalCommandValidator : AbstractValidator<PostJournalCommand>
{
    public PostJournalCommandValidator()
    {
        RuleFor(command => command.JournalId)
            .GreaterThan(0);
    }
}
