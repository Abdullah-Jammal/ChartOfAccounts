using Finance.Application.Common.Contracts.Auth;
using Finance.Application.Common.Exceptions;
using Finance.Application.Common.Interfaces.Auth;
using Finance.Infrastructure.Authentication.Configuration;
using Finance.Infrastructure.Authentication.Services;
using Finance.Infrastructure.Identity.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Finance.Infrastructure.Identity.Services;

internal class AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ApplicationDbContext dbContext,
    IJwtTokenProvider jwtTokenProvider,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider timeProvider) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthTokensResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());

        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthTokensResult> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var now = timeProvider.GetUtcNow();

        var refreshToken = await dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive(now))
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        if (refreshToken.User is null)
        {
            throw new UnauthorizedException("User no longer exists.");
        }

        return await IssueTokensAsync(refreshToken.User, cancellationToken, refreshToken, now);
    }

    public async Task<UserResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            throw new ConflictException($"A user with email '{email}' already exists.");
        }

        var user = ApplicationUser.Create(email);

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.SetFullName(request.FullName);
        }

        var createResult = await userManager.CreateAsync(user, request.Password);
        EnsureIdentitySuccess(createResult);

        var roles = request.Roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await EnsureRolesExistAsync(roles);

        if (roles.Length > 0)
        {
            var addRolesResult = await userManager.AddToRolesAsync(user, roles);
            EnsureIdentitySuccess(addRolesResult);
        }

        return new UserResult(
            user.Id,
            user.Email ?? email,
            user.FullName?.Value,
            roles);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);

        if (user is null)
        {
            throw new NotFoundException("User was not found.");
        }

        var changePasswordResult = await userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!changePasswordResult.Succeeded &&
            changePasswordResult.Errors.Any(error => error.Code == "PasswordMismatch"))
        {
            throw new UnauthorizedException("Current password is invalid.");
        }

        EnsureIdentitySuccess(changePasswordResult);
    }

    private async Task<AuthTokensResult> IssueTokensAsync(
        ApplicationUser user,
        CancellationToken cancellationToken,
        RefreshToken? currentRefreshToken = null,
        DateTimeOffset? nowOverride = null)
    {
        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        var (accessToken, accessTokenExpiresAtUtc) = jwtTokenProvider.CreateAccessToken(user, roles);

        var now = nowOverride ?? timeProvider.GetUtcNow();
        var refreshTokenValue = jwtTokenProvider.CreateRefreshToken();
        var refreshTokenExpiresAtUtc = now.AddDays(_jwtOptions.RefreshTokenLifetimeDays);
        var refreshTokenHash = HashToken(refreshTokenValue);

        if (currentRefreshToken is not null)
        {
            dbContext.RefreshTokens.Remove(currentRefreshToken);
        }

        dbContext.RefreshTokens.Add(
            RefreshToken.Create(user.Id, refreshTokenHash, now, refreshTokenExpiresAtUtc));

        await RemoveExpiredRefreshTokensAsync(user.Id, now, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthTokensResult(
            accessToken,
            accessTokenExpiresAtUtc,
            refreshTokenValue,
            refreshTokenExpiresAtUtc,
            new UserResult(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName?.Value,
                roles));
    }

    private async Task EnsureRolesExistAsync(IEnumerable<string> roles)
    {
        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(role));
            EnsureIdentitySuccess(result);
        }
    }

    private async Task RemoveExpiredRefreshTokensAsync(
        string userId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var staleTokens = await dbContext.RefreshTokens
            .Where(token => token.UserId == userId && (token.ExpiresAtUtc <= now || token.RevokedAtUtc != null))
            .ToListAsync(cancellationToken);

        if (staleTokens.Count == 0)
        {
            return;
        }

        dbContext.RefreshTokens.RemoveRange(staleTokens);
    }

    private static void EnsureIdentitySuccess(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var message = string.Join(", ", result.Errors.Select(error => error.Description));

        if (result.Errors.Any(error => error.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException(message);
        }

        throw new InvalidOperationException(message);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
