using Finance.Application.Features.Ledger.Dtos;
using Finance.Application.Features.Ledger.GetAccountLedger;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers.Ledger;

[ApiController]
[Route("api/ledger")]
public class LedgerController(ISender sender) : ControllerBase
{
    [HttpGet("{accountId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<LedgerEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LedgerEntryDto>>> GetAccountLedger(
        int accountId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetAccountLedgerQuery(accountId, fromDate, toDate, pageNumber, pageSize),
            cancellationToken);

        return Ok(result);
    }
}
