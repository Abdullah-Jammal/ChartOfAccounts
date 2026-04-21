using Finance.Application.Common.Exceptions;
using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Journals.Common;
using Finance.Application.Features.Journals.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Journals.AddJournalLine;

public record AddJournalLineCommand(
    int JournalId,
    int AccountId,
    decimal Debit,
    decimal Credit) : IRequest<JournalDto>;

public class AddJournalLineCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<AddJournalLineCommand, JournalDto>
{
    public async Task<JournalDto> Handle(AddJournalLineCommand request, CancellationToken cancellationToken)
    {
        var journal = await dbContext.Journals
            .Include(entry => entry.Lines)
            .SingleOrDefaultAsync(entry => entry.Id == request.JournalId, cancellationToken);

        if (journal is null)
        {
            throw new NotFoundException($"Journal {request.JournalId} was not found.");
        }

        var account = await dbContext.Accounts
            .SingleOrDefaultAsync(existingAccount => existingAccount.Id == request.AccountId, cancellationToken);

        if (account is null)
        {
            throw new NotFoundException($"Account {request.AccountId} was not found.");
        }

        journal.AddLine(account, request.Debit, request.Credit);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await JournalReadModels.GetJournalDtoAsync(dbContext, request.JournalId, cancellationToken);
    }
}
