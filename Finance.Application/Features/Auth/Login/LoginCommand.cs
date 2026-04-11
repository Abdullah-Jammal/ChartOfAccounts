using Finance.Application.Common.Contracts.Auth;
using Finance.Application.Common.Interfaces.Auth;
using MediatR;

namespace Finance.Application.Features.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthTokensResult>;

public class LoginCommandHandler(IAuthService authService) : IRequestHandler<LoginCommand, AuthTokensResult>
{
    public Task<AuthTokensResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return authService.LoginAsync(new LoginRequest(request.Email, request.Password), cancellationToken);
    }
}
