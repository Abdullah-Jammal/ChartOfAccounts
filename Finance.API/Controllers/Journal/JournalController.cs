using Finance.Application.Features.Journal.CreateJournalEntry;
using Finance.Application.Features.Journal.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers.Journal;

[ApiController]
[Route("api/journal")]
public class JournalController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(JournalEntryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<JournalEntryDto>> Create(
        CreateJournalEntryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }
}
