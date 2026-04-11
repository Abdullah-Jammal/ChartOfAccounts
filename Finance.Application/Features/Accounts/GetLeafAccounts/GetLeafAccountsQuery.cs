using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Accounts.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Accounts.GetLeafAccounts;

public record GetLeafAccountsQuery : IRequest<IReadOnlyList<AccountDto>>;

public class GetLeafAccountsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetLeafAccountsQuery, IReadOnlyList<AccountDto>>
{
    public async Task<IReadOnlyList<AccountDto>> Handle(
        GetLeafAccountsQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.IsPosting && account.IsActive)
            .OrderBy(account => account.Code)
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
