using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Transactions.Dtos;
using Finance.Domain.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Transactions.GetTransactionsList;

public record GetTransactionsListQuery(
    BusinessTransactionType? Type,
    BusinessTransactionStatus? Status,
    DateOnly? FromDate,
    DateOnly? ToDate,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<IReadOnlyList<BusinessTransactionDto>>;

public class GetTransactionsListQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTransactionsListQuery, IReadOnlyList<BusinessTransactionDto>>
{
    public async Task<IReadOnlyList<BusinessTransactionDto>> Handle(
        GetTransactionsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.BusinessTransactions.AsNoTracking();

        if (request.Type is not null)
        {
            query = query.Where(transaction => transaction.Type == request.Type.Value);
        }

        if (request.Status is not null)
        {
            query = query.Where(transaction => transaction.Status == request.Status.Value);
        }

        if (request.FromDate is not null)
        {
            query = query.Where(transaction => transaction.Date >= request.FromDate.Value);
        }

        if (request.ToDate is not null)
        {
            query = query.Where(transaction => transaction.Date <= request.ToDate.Value);
        }

        return await query
            .OrderByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(transaction => transaction.ToDto())
            .ToListAsync(cancellationToken);
    }
}
