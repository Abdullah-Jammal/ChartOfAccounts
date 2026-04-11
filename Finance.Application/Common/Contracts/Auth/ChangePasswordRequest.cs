namespace Finance.Application.Common.Contracts.Auth;

public record ChangePasswordRequest(
    string UserId,
    string CurrentPassword,
    string NewPassword);
