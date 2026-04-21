using Finance.Application.Features.Journals.AddJournalLine;
using Finance.Application.Features.Journals.CreateJournal;
using Finance.Application.Features.Journals.Dtos;
using Finance.Application.Features.Journals.GetJournalById;
using Finance.Application.Features.Journals.GetJournalsList;
using Finance.Application.Features.Journals.PostJournal;
using Finance.Application.Features.Journals.RemoveJournalLine;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers.Journals;

[ApiController]
[Route("api/journals")]
public class JournalsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(JournalDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<JournalDto>> Create(
        CreateJournalCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:int}/lines")]
    [ProducesResponseType(typeof(JournalDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<JournalDto>> AddLine(
        int id,
        AddJournalLineRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new AddJournalLineCommand(id, request.AccountId, request.Debit, request.Credit),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:int}/lines/{lineId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveLine(int id, int lineId, CancellationToken cancellationToken)
    {
        await sender.Send(new RemoveJournalLineCommand(id, lineId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/post")]
    [ProducesResponseType(typeof(JournalDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<JournalDto>> Post(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new PostJournalCommand(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(JournalDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<JournalDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetJournalByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<JournalSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<JournalSummaryDto>>> GetList(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetJournalsListQuery(), cancellationToken);
        return Ok(result);
    }

    public record AddJournalLineRequest(int AccountId, decimal Debit, decimal Credit);
}
