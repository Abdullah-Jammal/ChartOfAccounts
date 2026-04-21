using Finance.Domain.Common;

namespace Finance.Domain.Accounting;

public class Journal
{
    public const int MaxDescriptionLength = 1000;
    public const int MaxReferenceNumberLength = 100;
    public const int MaxCreatedByLength = 256;

    private readonly List<JournalLine> _lines = [];

    public int Id { get; private set; }
    public DateOnly Date { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? ReferenceNumber { get; private set; }
    public JournalStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public IReadOnlyCollection<JournalLine> Lines => _lines.AsReadOnly();
    public decimal TotalDebit => _lines.Sum(line => line.Debit);
    public decimal TotalCredit => _lines.Sum(line => line.Credit);

    private Journal()
    {
    }

    public static Journal CreateDraft(
        DateOnly date,
        string description,
        string? referenceNumber,
        DateTime createdAt,
        string createdBy)
    {
        if (date == default)
        {
            throw new DomainException("Journal date is required.");
        }

        var normalizedDescription = NormalizeRequired(description, nameof(Description), MaxDescriptionLength);
        var normalizedCreatedBy = NormalizeRequired(createdBy, nameof(CreatedBy), MaxCreatedByLength);
        var normalizedReferenceNumber = NormalizeOptional(referenceNumber, nameof(ReferenceNumber), MaxReferenceNumberLength);

        return new Journal
        {
            Date = date,
            Description = normalizedDescription,
            ReferenceNumber = normalizedReferenceNumber,
            Status = JournalStatus.Draft,
            CreatedAt = createdAt,
            CreatedBy = normalizedCreatedBy
        };
    }

    public JournalLine AddLine(Account account, decimal debit, decimal credit)
    {
        EnsureDraft();

        var line = JournalLine.Create(account, debit, credit);
        _lines.Add(line);

        return line;
    }

    public void RemoveLine(int lineId)
    {
        EnsureDraft();

        var line = _lines.SingleOrDefault(existingLine => existingLine.Id == lineId);

        if (line is null)
        {
            throw new DomainException($"Journal line {lineId} was not found.");
        }

        _lines.Remove(line);
    }

    public void Validate()
    {
        EnsureValidHeader();

        if (_lines.Count < 2)
        {
            throw new DomainException("Journal must contain at least two lines.");
        }

        foreach (var line in _lines)
        {
            line.Validate();
        }

        if (TotalDebit != TotalCredit)
        {
            throw new DomainException("Total debit must equal total credit.");
        }
    }

    public void Validate(IReadOnlyDictionary<int, Account> accountsById)
    {
        ArgumentNullException.ThrowIfNull(accountsById);

        Validate();

        foreach (var line in _lines)
        {
            line.Validate(accountsById);
        }
    }

    public void Post(IReadOnlyDictionary<int, Account> accountsById)
    {
        EnsureDraft();
        Validate(accountsById);
        Status = JournalStatus.Posted;
    }

    private void EnsureDraft()
    {
        if (Status == JournalStatus.Posted)
        {
            throw new DomainException("Posted journals cannot be modified.");
        }
    }

    private void EnsureValidHeader()
    {
        if (Date == default)
        {
            throw new DomainException("Journal date is required.");
        }

        _ = NormalizeRequired(Description, nameof(Description), MaxDescriptionLength);
        _ = NormalizeRequired(CreatedBy, nameof(CreatedBy), MaxCreatedByLength);
        _ = NormalizeOptional(ReferenceNumber, nameof(ReferenceNumber), MaxReferenceNumberLength);
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
