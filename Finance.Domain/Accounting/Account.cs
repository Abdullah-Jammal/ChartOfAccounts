using Finance.Domain.Common;

namespace Finance.Domain.Accounting;

public class Account
{
    private readonly List<Account> _children = [];

    public int Id { get; private set; }
    public int Code { get; private set; }
    public string NameAr { get; private set; } = string.Empty;
    public string? NameEn { get; private set; }
    public int Level { get; private set; }
    public int? ParentId { get; private set; }
    public Account? Parent { get; private set; }
    public IReadOnlyCollection<Account> Children => _children.AsReadOnly();
    public AccountType Type { get; private set; }
    public bool IsPosting { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Account()
    {
    }

    public static Account Create(
        int code,
        string nameAr,
        string? nameEn,
        int level,
        int? parentId,
        AccountType type,
        bool isPosting,
        bool isActive,
        DateTime createdAt)
    {
        return new Account
        {
            Code = code,
            NameAr = nameAr.Trim(),
            NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim(),
            Level = level,
            ParentId = parentId,
            Type = type,
            IsPosting = isPosting,
            IsActive = isActive,
            CreatedAt = createdAt
        };
    }

    public void EnsureCanPost()
    {
        if (!IsPosting)
        {
            throw new DomainException($"Account {Code} is not a posting account.");
        }

        if (!IsActive)
        {
            throw new DomainException($"Account {Code} is inactive.");
        }
    }
}
