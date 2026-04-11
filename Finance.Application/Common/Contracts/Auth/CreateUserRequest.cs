namespace Finance.Application.Common.Contracts.Auth;

public record CreateUserRequest(
    string Email,
    string Password,
    string? FullName,
    IReadOnlyCollection<string> Roles);
