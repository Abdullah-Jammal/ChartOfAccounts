using Finance.Application.Common.Interfaces.Accounting;
using Finance.Application.Common.Interfaces.Persistence;
using Finance.Domain.Accounting;
using Finance.Domain.Common;
using Finance.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Accounting;

public class JournalGenerator(IApplicationDbContext dbContext) : IJournalGenerator
{
    public async Task<Journal> GenerateAndPostAsync(
        BusinessTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (transaction.Status != BusinessTransactionStatus.Pending)
        {
            throw new DomainException("Only pending transactions can generate journals.");
        }

        var accountIds = new[] { transaction.DebitAccountId, transaction.CreditAccountId };
        var accountsById = await dbContext.Accounts
            .Where(account => accountIds.Contains(account.Id))
            .ToDictionaryAsync(account => account.Id, cancellationToken);

        if (!accountsById.TryGetValue(transaction.DebitAccountId, out var debitAccount))
        {
            throw new DomainException($"Debit account {transaction.DebitAccountId} was not found.");
        }

        if (!accountsById.TryGetValue(transaction.CreditAccountId, out var creditAccount))
        {
            throw new DomainException($"Credit account {transaction.CreditAccountId} was not found.");
        }

        ValidateMapping(transaction, debitAccount, creditAccount);

        var journal = Journal.CreateDraft(
            transaction.Date,
            BuildJournalDescription(transaction),
            transaction.ReferenceId ?? $"TX-{transaction.Id}",
            transaction.CreatedAt,
            transaction.CreatedBy);

        journal.AddLine(debitAccount, transaction.Amount, 0m);
        journal.AddLine(creditAccount, 0m, transaction.Amount);
        journal.Post(accountsById);

        dbContext.Journals.Add(journal);
        await dbContext.SaveChangesAsync(cancellationToken);

        return journal;
    }

    private static void ValidateMapping(
        BusinessTransaction transaction,
        Account debitAccount,
        Account creditAccount)
    {
        debitAccount.EnsureCanPost();
        creditAccount.EnsureCanPost();

        switch (transaction.Type)
        {
            case BusinessTransactionType.Payment:
                EnsureType(debitAccount, AccountType.Asset, "Payment cash/bank account");
                EnsureType(creditAccount, AccountType.Revenue, "Payment revenue account");
                break;

            case BusinessTransactionType.Expense:
                EnsureType(debitAccount, AccountType.Expense, "Expense account");
                EnsureType(creditAccount, AccountType.Asset, "Expense payment account");
                break;

            case BusinessTransactionType.Transfer:
                EnsureType(debitAccount, AccountType.Asset, "Transfer destination account");
                EnsureType(creditAccount, AccountType.Asset, "Transfer source account");
                break;

            default:
                throw new DomainException($"Transaction type {transaction.Type} is not supported.");
        }
    }

    private static void EnsureType(Account account, AccountType expectedType, string role)
    {
        if (account.Type != expectedType)
        {
            throw new DomainException($"{role} must be a posting {expectedType} account.");
        }
    }

    private static string BuildJournalDescription(BusinessTransaction transaction)
    {
        return $"{transaction.Type} transaction #{transaction.Id}: {transaction.Description}";
    }
}
