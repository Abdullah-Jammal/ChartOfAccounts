using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Common.Interfaces.User;
using Finance.Application.Features.Journals.Dtos;
using Finance.Domain.Accounting;
using MediatR;

namespace Finance.Application.Features.Journals.CreateJournal;

public record CreateJournalCommand(
    DateOnly Date,
    string Description,
    string? ReferenceNumber) : IRequest<JournalDto>;

public class CreateJournalCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider) : IRequestHandler<CreateJournalCommand, JournalDto>
{
    public async Task<JournalDto> Handle(CreateJournalCommand request, CancellationToken cancellationToken)
    {
        var createdBy = currentUserService.Email
            ?? currentUserService.UserId
            ?? "system";

        var journal = Journal.CreateDraft(
            request.Date,
            request.Description,
            request.ReferenceNumber,
            timeProvider.GetUtcNow().UtcDateTime,
            createdBy);

        dbContext.Journals.Add(journal);
        await dbContext.SaveChangesAsync(cancellationToken);

        return journal.ToDto();
    }
}
