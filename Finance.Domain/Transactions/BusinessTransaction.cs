using Finance.Domain.Accounting;
using Finance.Domain.Common;

namespace Finance.Domain.Transactions;

public class BusinessTransaction
{
    public const int MaxDescriptionLength = Journal.MaxDescriptionLength;
    public const int MaxReferenceIdLength = 100;
    public const int MaxCreatedByLength = Journal.MaxCreatedByLength;
    public const int MaxFailureReasonLength = 1000;

    public int Id { get; private set; }
    public BusinessTransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly Date { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? ReferenceId { get; private set; }
    public BusinessTransactionStatus Status { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public int DebitAccountId { get; private set; }
    public Account DebitAccount { get; private set; } = null!;
    public int CreditAccountId { get; private set; }
    public Account CreditAccount { get; private set; } = null!;
    public int? JournalId { get; private set; }
    public Journal? Journal { get; private set; }

    private BusinessTransaction()
    {
    }

    public static BusinessTransaction CreatePending(
        BusinessTransactionType type,
        decimal amount,
        DateOnly date,
        string description,
        string? referenceId,
        int debitAccountId,
        int creditAccountId,
        string createdBy,
        DateTime createdAt)
    {
        if (date == default)
        {
            throw new DomainException("Transaction date is required.");
        }

        if (amount <= 0m)
        {
            throw new DomainException("Transaction amount must be greater than zero.");
        }

        if (decimal.Round(amount, 2, MidpointRounding.AwayFromZero) != amount)
        {
            throw new DomainException("Transaction amount cannot have more than 2 decimal places.");
        }

        if (debitAccountId <= 0 || creditAccountId <= 0)
        {
            throw new DomainException("Debit and credit accounts are required.");
        }

        if (debitAccountId == creditAccountId)
        {
            throw new DomainException("Debit and credit accounts must be different.");
        }

        return new BusinessTransaction
        {
            Type = type,
            Amount = amount,
            Date = date,
            Description = NormalizeRequired(description, nameof(Description), MaxDescriptionLength),
            ReferenceId = NormalizeOptional(referenceId, nameof(ReferenceId), MaxReferenceIdLength),
            Status = BusinessTransactionStatus.Pending,
            CreatedBy = NormalizeRequired(createdBy, nameof(CreatedBy), MaxCreatedByLength),
            CreatedAt = createdAt,
            DebitAccountId = debitAccountId,
            CreditAccountId = creditAccountId
        };
    }

    public void Complete(Journal journal, DateTime completedAt)
    {
        ArgumentNullException.ThrowIfNull(journal);

        if (Status != BusinessTransactionStatus.Pending)
        {
            throw new DomainException("Only pending transactions can be completed.");
        }

        if (journal.Status != JournalStatus.Posted)
        {
            throw new DomainException("A transaction cannot be completed without a posted journal.");
        }

        Status = BusinessTransactionStatus.Completed;
        JournalId = journal.Id;
        CompletedAt = completedAt;
        FailedAt = null;
        FailureReason = null;
    }

    public void Fail(string reason, DateTime failedAt)
    {
        if (Status != BusinessTransactionStatus.Pending)
        {
            throw new DomainException("Only pending transactions can fail.");
        }

        Status = BusinessTransactionStatus.Failed;
        FailedAt = failedAt;
        FailureReason = NormalizeRequired(reason, nameof(FailureReason), MaxFailureReasonLength);
    }

    private static string NormalizeRequired(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > maxLength)
        {
            throw new DomainException($"{fieldName} cannot exceed {maxLength} characters.");
        }

        return trimmedValue;
    }

    private static string? NormalizeOptional(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > maxLength)
        {
            throw new DomainException($"{fieldName} cannot exceed {maxLength} characters.");
        }

        return trimmedValue;
    }
}
