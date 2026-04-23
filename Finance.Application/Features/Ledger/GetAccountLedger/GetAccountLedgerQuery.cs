using Finance.Application.Common.Exceptions;
using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Ledger.Dtos;
using Finance.Domain.Accounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Ledger.GetAccountLedger;

public record GetAccountLedgerQuery(
    int AccountId,
    DateOnly? FromDate,
    DateOnly? ToDate,
    int? PageNumber,
    int? PageSize) : IRequest<IReadOnlyList<LedgerEntryDto>>;

public class GetAccountLedgerQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAccountLedgerQuery, IReadOnlyList<LedgerEntryDto>>
{
    public async Task<IReadOnlyList<LedgerEntryDto>> Handle(
        GetAccountLedgerQuery request,
        CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts
            .AsNoTracking()
            .SingleOrDefaultAsync(account => account.Id == request.AccountId, cancellationToken);

        if (account is null)
        {
            throw new NotFoundException($"Account {request.AccountId} was not found.");
        }

        var query = dbContext.JournalLines
            .AsNoTracking()
            .Where(line => line.AccountId == request.AccountId &&
                line.Journal.Status == JournalStatus.Posted);

        if (request.ToDate is not null)
        {
            query = query.Where(line => line.Journal.Date <= request.ToDate.Value);
        }

        var movements = await query
            .OrderBy(line => line.Journal.Date)
            .ThenBy(line => line.JournalId)
            .ThenBy(line => line.Id)
            .Select(line => new LedgerMovement(
                line.AccountId,
                line.Account.NameAr,
                line.JournalId,
                line.Journal.Date,
                line.Journal.Description,
                line.Debit,
                line.Credit))
            .ToListAsync(cancellationToken);

        var ledgerEntries = new List<LedgerEntryDto>(movements.Count);
        var runningBalance = 0m;

        foreach (var movement in movements)
        {
            runningBalance += movement.Debit - movement.Credit;

            if (request.FromDate is not null && movement.Date < request.FromDate.Value)
            {
                continue;
            }

            ledgerEntries.Add(new LedgerEntryDto(
                movement.AccountId,
                movement.AccountName,
                movement.JournalId,
                movement.Date,
                movement.Description,
                movement.Debit,
                movement.Credit,
                runningBalance));
        }

        if (request.PageNumber is null || request.PageSize is null)
        {
            return ledgerEntries;
        }

        return ledgerEntries
            .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
            .Take(request.PageSize.Value)
            .ToList();
    }

    private sealed record LedgerMovement(
        int AccountId,
        string AccountName,
        int JournalId,
        DateOnly Date,
        string Description,
        decimal Debit,
        decimal Credit);
}
