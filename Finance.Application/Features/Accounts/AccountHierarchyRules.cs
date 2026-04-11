using Finance.Domain.Accounting;

namespace Finance.Application.Features.Accounts;

internal static class AccountHierarchyRules
{
    public static bool TryGetTypeFromCode(int code, out AccountType type)
    {
        type = default;

        if (code <= 0)
        {
            return false;
        }

        type = code.ToString()[0] switch
        {
            '1' => AccountType.Asset,
            '2' => AccountType.Liability,
            '3' => AccountType.Expense,
            '4' => AccountType.Revenue,
            _ => default
        };

        return type is not default(AccountType);
    }

    public static AccountType GetTypeFromCode(int code)
    {
        if (TryGetTypeFromCode(code, out var type))
        {
            return type;
        }

        throw new InvalidOperationException("Only root accounts 1-4 are active in the current COA model.");
    }
}
