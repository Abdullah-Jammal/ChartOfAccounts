using Finance.Application.Common.Exceptions;
using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Transactions.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Transactions.GetTransactionById;

public record GetTransactionByIdQuery(int TransactionId) : IRequest<BusinessTransactionDto>;

public class GetTransactionByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTransactionByIdQuery, BusinessTransactionDto>
{
    public async Task<BusinessTransactionDto> Handle(
        GetTransactionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var transaction = await dbContext.BusinessTransactions
            .AsNoTracking()
            .SingleOrDefaultAsync(transaction => transaction.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
        {
            throw new NotFoundException($"Transaction {request.TransactionId} was not found.");
        }

        return transaction.ToDto();
    }
}
