using Finance.Application.Common.Contracts.Auth;

namespace Finance.Application.Common.Interfaces.Auth;

public interface IAuthService
{
    Task<AuthTokensResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthTokensResult> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task<UserResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
}
