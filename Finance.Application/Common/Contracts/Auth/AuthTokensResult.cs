namespace Finance.Application.Common.Contracts.Auth;

public record AuthTokensResult(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    UserResult User);
