using Finance.Application.Common.Interfaces.Persistence;
using Finance.Domain.Accounting;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Reports.Common;

internal static class PostedAccountMovementReader
{
    public static async Task<IReadOnlyList<AccountMovementSummary>> GetSummariesAsync(
        IApplicationDbContext dbContext,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var query = dbContext.JournalLines
            .AsNoTracking()
            .Where(line => line.Journal.Status == JournalStatus.Posted);

        if (fromDate is not null)
        {
            query = query.Where(line => line.Journal.Date >= fromDate.Value);
        }

        if (toDate is not null)
        {
            query = query.Where(line => line.Journal.Date <= toDate.Value);
        }

        return await query
            .GroupBy(line => new
            {
                line.AccountId,
                line.Account.Code,
                line.Account.NameAr,
                line.Account.Type
            })
            .Select(group => new AccountMovementSummary(
                group.Key.AccountId,
                group.Key.Code,
                group.Key.NameAr,
                group.Key.Type,
                group.Sum(line => line.Debit),
                group.Sum(line => line.Credit)))
            .OrderBy(summary => summary.AccountCode)
            .ToListAsync(cancellationToken);
    }
}

internal sealed record AccountMovementSummary(
    int AccountId,
    int AccountCode,
    string AccountName,
    AccountType AccountType,
    decimal DebitTotal,
    decimal CreditTotal)
{
    public decimal DebitMinusCredit => DebitTotal - CreditTotal;
    public decimal CreditMinusDebit => CreditTotal - DebitTotal;
}
