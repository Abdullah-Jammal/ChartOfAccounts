using Finance.Application.Features.Accounts.CreateAccount;
using Finance.Application.Features.Accounts.DeleteAccount;
using Finance.Application.Features.Accounts.Dtos;
using Finance.Application.Features.Accounts.GetAccountsTree;
using Finance.Application.Features.Accounts.GetLeafAccounts;
using Finance.Application.Features.Accounts.SearchAccounts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers.Accounts;

[ApiController]
[Route("api/accounts")]
public class AccountsController(ISender sender) : ControllerBase
{
    [HttpGet("tree")]
    [ProducesResponseType(typeof(IReadOnlyList<AccountTreeNodeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AccountTreeNodeDto>>> GetTree(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAccountsTreeQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("leaf")]
    [ProducesResponseType(typeof(IReadOnlyList<AccountDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> GetLeaf(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLeafAccountsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<AccountDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> Search(
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchAccountsQuery(query), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AccountDto>> Create(
        CreateAccountCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Search), new { query = result.Code }, result);
    }

    [HttpDelete("{code:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int code, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteAccountCommand(code), cancellationToken);
        return NoContent();
    }
}
