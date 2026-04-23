using Finance.Application.Common.Interfaces.Persistence;
using Finance.Domain.Accounting;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Reports.Common;

internal static class CashAccountReader
{
    public static async Task<IReadOnlyList<int>> GetCashAccountIdsAsync(
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.Type == AccountType.Asset &&
                account.IsPosting &&
                account.IsActive)
            .Select(account => new
            {
                account.Id,
                account.Code,
                account.NameAr,
                account.NameEn
            })
            .ToListAsync(cancellationToken);

        return accounts
            .Where(account => IsCashOrBankAccount(account.Code, account.NameAr, account.NameEn))
            .Select(account => account.Id)
            .ToArray();
    }

    private static bool IsCashOrBankAccount(int code, string nameAr, string? nameEn)
    {
        var codeText = code.ToString();

        if (codeText.StartsWith("181", StringComparison.Ordinal) ||
            codeText.StartsWith("183", StringComparison.Ordinal) ||
            codeText.StartsWith("184", StringComparison.Ordinal))
        {
            return true;
        }

        var searchableName = $"{nameAr} {nameEn}".ToLowerInvariant();

        return searchableName.Contains("cash", StringComparison.Ordinal) ||
            searchableName.Contains("bank", StringComparison.Ordinal) ||
            searchableName.Contains("صندوق", StringComparison.Ordinal) ||
            searchableName.Contains("مصرف", StringComparison.Ordinal) ||
            searchableName.Contains("بنك", StringComparison.Ordinal);
    }
}
