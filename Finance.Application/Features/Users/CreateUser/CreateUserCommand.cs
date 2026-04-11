using Finance.Application.Common.Constants;
using Finance.Application.Common.Contracts.Auth;
using Finance.Application.Common.Interfaces.Auth;
using MediatR;

namespace Finance.Application.Features.Users.CreateUser;

public record CreateUserCommand(
    string Email,
    string Password,
    string? FullName,
    IReadOnlyCollection<string>? Roles) : IRequest<UserResult>;

public class CreateUserCommandHandler(IAuthService authService)
    : IRequestHandler<CreateUserCommand, UserResult>
{
    public Task<UserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var roles = request.Roles?.Count > 0
            ? request.Roles
            : [Roles.User];

        return authService.CreateUserAsync(
            new CreateUserRequest(request.Email, request.Password, request.FullName, roles),
            cancellationToken);
    }
}
