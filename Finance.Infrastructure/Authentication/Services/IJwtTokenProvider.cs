using Finance.Infrastructure.Identity.Entity;
using System.Security.Claims;

namespace Finance.Infrastructure.Authentication.Services;

internal interface IJwtTokenProvider
{
    (string Token, DateTimeOffset ExpiresAtUtc) CreateAccessToken(
        ApplicationUser user,
        IReadOnlyCollection<string> roles);

    string CreateRefreshToken();

    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
}
