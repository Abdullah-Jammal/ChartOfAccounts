namespace Finance.Application.Features.Accounts.Dtos;

public record AccountTreeNodeDto(
    int Id,
    int Code,
    string NameAr,
    string? NameEn,
    int Level,
    int? ParentId,
    int? ParentCode,
    string Type,
    bool IsPosting,
    bool IsActive,
    IReadOnlyList<AccountTreeNodeDto> Children);
