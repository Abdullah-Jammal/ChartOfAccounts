using Finance.Application.Common.Exceptions;
using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Journals.Common;
using Finance.Application.Features.Journals.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Journals.PostJournal;

public record PostJournalCommand(int JournalId) : IRequest<JournalDto>;

public class PostJournalCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<PostJournalCommand, JournalDto>
{
    public async Task<JournalDto> Handle(PostJournalCommand request, CancellationToken cancellationToken)
    {
        var journalId = await dbContext.ExecuteInTransactionAsync(
            async transactionCancellationToken =>
            {
                var journal = await dbContext.Journals
                    .Include(entry => entry.Lines)
                    .SingleOrDefaultAsync(entry => entry.Id == request.JournalId, transactionCancellationToken);

                if (journal is null)
                {
                    throw new NotFoundException($"Journal {request.JournalId} was not found.");
                }

                var accountIds = journal.Lines
                    .Select(line => line.AccountId)
                    .Distinct()
                    .ToArray();

                var accountsById = await dbContext.Accounts
                    .Where(account => accountIds.Contains(account.Id))
                    .ToDictionaryAsync(account => account.Id, transactionCancellationToken);

                journal.Post(accountsById);
                await dbContext.SaveChangesAsync(transactionCancellationToken);

                return journal.Id;
            },
            cancellationToken);

        return await JournalReadModels.GetJournalDtoAsync(dbContext, journalId, cancellationToken);
    }
}
