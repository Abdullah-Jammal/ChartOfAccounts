using Finance.Application.Common.Exceptions;
using Finance.Application.Common.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Journals.RemoveJournalLine;

public record RemoveJournalLineCommand(int JournalId, int LineId) : IRequest;

public class RemoveJournalLineCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<RemoveJournalLineCommand>
{
    public async Task Handle(RemoveJournalLineCommand request, CancellationToken cancellationToken)
    {
        var journal = await dbContext.Journals
            .Include(entry => entry.Lines)
            .SingleOrDefaultAsync(entry => entry.Id == request.JournalId, cancellationToken);

        if (journal is null)
        {
            throw new NotFoundException($"Journal {request.JournalId} was not found.");
        }

        journal.RemoveLine(request.LineId);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
