using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Accounts.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Accounts.SearchAccounts;

public record SearchAccountsQuery(string Query) : IRequest<IReadOnlyList<AccountDto>>;

public class SearchAccountsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<SearchAccountsQuery, IReadOnlyList<AccountDto>>
{
    private const int MaxResults = 50;

    public async Task<IReadOnlyList<AccountDto>> Handle(
        SearchAccountsQuery request,
        CancellationToken cancellationToken)
    {
        var query = request.Query.Trim();

        return await dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.NameAr.Contains(query) || account.Code.ToString().Contains(query))
            .OrderBy(account => account.Code)
            .Take(MaxResults)
            .Select(account => new AccountDto(
                account.Id,
                account.Code,
                account.NameAr,
                account.NameEn,
                account.Level,
                account.ParentId,
                account.Parent == null ? null : account.Parent.Code,
                account.Type.ToString(),
                account.IsPosting,
                account.IsActive))
            .ToListAsync(cancellationToken);
    }
}
