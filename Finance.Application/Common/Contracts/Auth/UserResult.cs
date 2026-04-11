namespace Finance.Application.Common.Contracts.Auth;

public record UserResult(
    string Id,
    string Email,
    string? FullName,
    IReadOnlyCollection<string> Roles);
