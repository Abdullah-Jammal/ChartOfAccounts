using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Journal.Dtos;
using Finance.Domain.Accounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Journal.CreateJournalEntry;

public record CreateJournalEntryCommand(
    DateOnly Date,
    IReadOnlyCollection<CreateJournalEntryLineRequest> Lines) : IRequest<JournalEntryDto>;

public record CreateJournalEntryLineRequest(
    int AccountId,
    decimal Debit,
    decimal Credit);

public class CreateJournalEntryCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateJournalEntryCommand, JournalEntryDto>
{
    public async Task<JournalEntryDto> Handle(
        CreateJournalEntryCommand request,
        CancellationToken cancellationToken)
    {
        var lines = request.Lines
            .Select(line => JournalEntryLine.Create(line.AccountId, line.Debit, line.Credit))
            .ToList();

        var journalEntry = JournalEntry.Create(request.Date, lines);

        dbContext.JournalEntries.Add(journalEntry);
        await dbContext.SaveChangesAsync(cancellationToken);

        var accountIds = lines
            .Select(line => line.AccountId)
            .Distinct()
            .ToArray();

        var accountLookup = await dbContext.Accounts
            .AsNoTracking()
            .Where(account => accountIds.Contains(account.Id))
            .ToDictionaryAsync(account => account.Id, cancellationToken);

        return new JournalEntryDto(
            journalEntry.Id,
            journalEntry.Date,
            lines.Sum(line => line.Debit),
            lines.Sum(line => line.Credit),
            lines.Select(line =>
            {
                var account = accountLookup[line.AccountId];

                return new JournalEntryLineDto(
                    line.Id,
                    line.AccountId,
                    account.Code,
                    account.NameAr,
                    line.Debit,
                    line.Credit);
            }).ToList());
    }
}
