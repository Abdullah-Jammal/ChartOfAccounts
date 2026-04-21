using Finance.Domain.Common;

namespace Finance.Domain.Accounting;

public class JournalLine
{
    public int Id { get; private set; }
    public int JournalId { get; private set; }
    public Journal Journal { get; private set; } = null!;
    public int AccountId { get; private set; }
    public Account Account { get; private set; } = null!;
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }

    private JournalLine()
    {
    }

    internal static JournalLine Create(Account account, decimal debit, decimal credit)
    {
        ArgumentNullException.ThrowIfNull(account);

        account.EnsureCanPost();
        ValidateAmounts(debit, credit);

        return new JournalLine
        {
            AccountId = account.Id,
            Debit = debit,
            Credit = credit
        };
    }

    internal void Validate(IReadOnlyDictionary<int, Account>? accountsById = null)
    {
        ValidateAmounts(Debit, Credit);

        if (accountsById is null)
        {
            return;
        }

        if (!accountsById.TryGetValue(AccountId, out var account))
        {
            throw new DomainException($"Account {AccountId} was not found.");
        }

        account.EnsureCanPost();
    }

    private static void ValidateAmounts(decimal debit, decimal credit)
    {
        EnsureNonNegative(debit, nameof(Debit));
        EnsureNonNegative(credit, nameof(Credit));
        EnsureCurrencyPrecision(debit, nameof(Debit));
        EnsureCurrencyPrecision(credit, nameof(Credit));

        if (debit > 0m && credit > 0m)
        {
            throw new DomainException("A journal line cannot contain both debit and credit amounts.");
        }

        if (debit == 0m && credit == 0m)
        {
            throw new DomainException("A journal line must contain either a debit amount or a credit amount.");
        }
    }

    private static void EnsureNonNegative(decimal amount, string name)
    {
        if (amount < 0m)
        {
            throw new DomainException($"{name} cannot be negative.");
        }
    }

    private static void EnsureCurrencyPrecision(decimal amount, string name)
    {
        if (decimal.Round(amount, 2, MidpointRounding.AwayFromZero) != amount)
        {
            throw new DomainException($"{name} cannot have more than 2 decimal places.");
        }
    }
}
