using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Accounts.Dtos;
using Finance.Domain.Accounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Accounts.GetAccountsTree;

public record GetAccountsTreeQuery : IRequest<IReadOnlyList<AccountTreeNodeDto>>;

public class GetAccountsTreeQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAccountsTreeQuery, IReadOnlyList<AccountTreeNodeDto>>
{
    public async Task<IReadOnlyList<AccountTreeNodeDto>> Handle(
        GetAccountsTreeQuery request,
        CancellationToken cancellationToken)
    {
        var accounts = await dbContext.Accounts
            .AsNoTracking()
            .OrderBy(account => account.Level)
            .ThenBy(account => account.Code)
            .Select(account => new AccountTreeFlatDto(
                account.Id,
                account.Code,
                account.NameAr,
                account.NameEn,
                account.Level,
                account.ParentId,
                account.Type,
                account.IsPosting,
                account.IsActive))
            .ToListAsync(cancellationToken);

        var codeById = accounts.ToDictionary(account => account.Id, account => account.Code);
        var childrenByParent = accounts
            .GroupBy(account => account.ParentId ?? 0)
            .ToDictionary(group => group.Key, group => group.OrderBy(account => account.Code).ToList());

        return BuildChildren(0, childrenByParent, codeById);
    }

    private static IReadOnlyList<AccountTreeNodeDto> BuildChildren(
        int parentId,
        IReadOnlyDictionary<int, List<AccountTreeFlatDto>> childrenByParent,
        IReadOnlyDictionary<int, int> codeById)
    {
        if (!childrenByParent.TryGetValue(parentId, out var children))
        {
            return [];
        }

        return children
            .Select(account => new AccountTreeNodeDto(
                account.Id,
                account.Code,
                account.NameAr,
                account.NameEn,
                account.Level,
                account.ParentId,
                account.ParentId is null ? null : codeById[account.ParentId.Value],
                account.Type.ToString(),
                account.IsPosting,
                account.IsActive,
                BuildChildren(account.Id, childrenByParent, codeById)))
            .ToList();
    }

    private sealed record AccountTreeFlatDto(
        int Id,
        int Code,
        string NameAr,
        string? NameEn,
        int Level,
        int? ParentId,
        AccountType Type,
        bool IsPosting,
        bool IsActive);
}
