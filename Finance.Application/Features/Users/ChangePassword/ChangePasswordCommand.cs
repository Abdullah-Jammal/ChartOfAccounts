using Finance.Application.Common.Contracts.Auth;
using Finance.Application.Common.Exceptions;
using Finance.Application.Common.Interfaces.Auth;
using Finance.Application.Common.Interfaces.User;
using MediatR;

namespace Finance.Application.Features.Users.ChangePassword;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<Unit>;

public class ChangePasswordCommandHandler(
    ICurrentUserService currentUserService,
    IAuthService authService) : IRequestHandler<ChangePasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(currentUserService.UserId))
        {
            throw new UnauthorizedException("Authentication is required to change the password.");
        }

        await authService.ChangePasswordAsync(
            new ChangePasswordRequest(currentUserService.UserId, request.CurrentPassword, request.NewPassword),
            cancellationToken);

        return Unit.Value;
    }
}
