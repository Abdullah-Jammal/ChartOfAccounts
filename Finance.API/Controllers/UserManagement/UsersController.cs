using Finance.Application.Common.Constants;
using Finance.Application.Common.Contracts.Auth;
using Finance.Application.Features.Users.ChangePassword;
using Finance.Application.Features.Users.CreateUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers.UserManagement;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.SuperAdmin)]
    [ProducesResponseType(typeof(UserResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<UserResult>> Create(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }

    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return NoContent();
    }
}
