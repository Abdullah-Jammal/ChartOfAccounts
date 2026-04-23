using Finance.Application.Common.Exceptions;
using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Accounts.Dtos;
using Finance.Domain.Accounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Accounts.GetAccountBalance;

public record GetAccountBalanceQuery(
    int AccountId,
    DateOnly? AsOfDate,
    bool IncludeChildren = true) : IRequest<AccountBalanceDto>;

public class GetAccountBalanceQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAccountBalanceQuery, AccountBalanceDto>
{
    public async Task<AccountBalanceDto> Handle(
        GetAccountBalanceQuery request,
        CancellationToken cancellationToken)
    {
        var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Select(account => new AccountSnapshot(
                account.Id,
                account.Code,
                account.NameAr,
                account.Type,
                account.ParentId,
                account.IsPosting))
            .ToListAsync(cancellationToken);

        var account = accounts.SingleOrDefault(account => account.Id == request.AccountId);

        if (account is null)
        {
            throw new NotFoundException($"Account {request.AccountId} was not found.");
        }

        var accountIds = request.IncludeChildren
            ? GetAccountAndDescendantIds(accounts, request.AccountId)
            : [request.AccountId];

        var query = dbContext.JournalLines
            .AsNoTracking()
            .Where(line => accountIds.Contains(line.AccountId) &&
                line.Journal.Status == JournalStatus.Posted);

        if (request.AsOfDate is not null)
        {
            query = query.Where(line => line.Journal.Date <= request.AsOfDate.Value);
        }

        var totals = await query
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Debit = group.Sum(line => line.Debit),
                Credit = group.Sum(line => line.Credit)
            })
            .SingleOrDefaultAsync(cancellationToken);

        var debitTotal = totals?.Debit ?? 0m;
        var creditTotal = totals?.Credit ?? 0m;

        return new AccountBalanceDto(
            account.Id,
            account.Code,
            account.Name,
            account.Type.ToString(),
            request.AsOfDate,
            debitTotal,
            creditTotal,
            debitTotal - creditTotal,
            accountIds.Count > 1);
    }

    private static IReadOnlyList<int> GetAccountAndDescendantIds(
        IReadOnlyCollection<AccountSnapshot> accounts,
        int accountId)
    {
        var result = new List<int> { accountId };
        var queue = new Queue<int>();
        queue.Enqueue(accountId);

        while (queue.Count > 0)
        {
            var parentId = queue.Dequeue();
            var childIds = accounts
                .Where(account => account.ParentId == parentId)
                .Select(account => account.Id)
                .ToArray();

            foreach (var childId in childIds)
            {
                result.Add(childId);
                queue.Enqueue(childId);
            }
        }

        return result;
    }

    private sealed record AccountSnapshot(
        int Id,
        int Code,
        string Name,
        AccountType Type,
        int? ParentId,
        bool IsPosting);
}
