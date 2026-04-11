using Finance.Application.Common.Contracts.Auth;
using Finance.Application.Common.Interfaces.Auth;
using MediatR;

namespace Finance.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthTokensResult>;

public class RefreshTokenCommandHandler(IAuthService authService)
    : IRequestHandler<RefreshTokenCommand, AuthTokensResult>
{
    public Task<AuthTokensResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return authService.RefreshTokenAsync(
            new RefreshTokenRequest(request.RefreshToken),
            cancellationToken);
    }
}
