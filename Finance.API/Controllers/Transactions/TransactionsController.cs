using Finance.Application.Features.Transactions.CreateExpenseTransaction;
using Finance.Application.Features.Transactions.CreatePaymentTransaction;
using Finance.Application.Features.Transactions.CreateTransferTransaction;
using Finance.Application.Features.Transactions.Dtos;
using Finance.Application.Features.Transactions.GetTransactionById;
using Finance.Application.Features.Transactions.GetTransactionsList;
using Finance.Domain.Transactions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers.Transactions;

[ApiController]
[Route("api/transactions")]
public class TransactionsController(ISender sender) : ControllerBase
{
    [HttpPost("payments")]
    [ProducesResponseType(typeof(BusinessTransactionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<BusinessTransactionDto>> CreatePayment(
        CreatePaymentTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("expenses")]
    [ProducesResponseType(typeof(BusinessTransactionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<BusinessTransactionDto>> CreateExpense(
        CreateExpenseTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("transfers")]
    [ProducesResponseType(typeof(BusinessTransactionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<BusinessTransactionDto>> CreateTransfer(
        CreateTransferTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BusinessTransactionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BusinessTransactionDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTransactionByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BusinessTransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BusinessTransactionDto>>> GetList(
        [FromQuery] BusinessTransactionType? type,
        [FromQuery] BusinessTransactionStatus? status,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetTransactionsListQuery(
                type,
                status,
                fromDate,
                toDate,
                pageNumber == 0 ? 1 : pageNumber,
                pageSize == 0 ? 50 : pageSize),
            cancellationToken);

        return Ok(result);
    }
}
