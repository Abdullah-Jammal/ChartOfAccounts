using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Reports.Common;
using Finance.Application.Features.Reports.Dtos;
using Finance.Domain.Accounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Reports.GetCashFlow;

public record GetCashFlowQuery(DateOnly? FromDate, DateOnly? ToDate)
    : IRequest<CashFlowSummaryReportDto>;

public class GetCashFlowQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetCashFlowQuery, CashFlowSummaryReportDto>
{
    public async Task<CashFlowSummaryReportDto> Handle(
        GetCashFlowQuery request,
        CancellationToken cancellationToken)
    {
        var cashAccountIds = await CashAccountReader.GetCashAccountIdsAsync(dbContext, cancellationToken);

        var query = dbContext.JournalLines
            .AsNoTracking()
            .Where(line => cashAccountIds.Contains(line.AccountId) &&
                line.Journal.Status == JournalStatus.Posted);

        if (request.FromDate is not null)
        {
            query = query.Where(line => line.Journal.Date >= request.FromDate.Value);
        }

        if (request.ToDate is not null)
        {
            query = query.Where(line => line.Journal.Date <= request.ToDate.Value);
        }

        var accounts = await query
            .GroupBy(line => new
            {
                line.AccountId,
                line.Account.Code,
                line.Account.NameAr
            })
            .Select(group => new CashFlowAccountLineDto(
                group.Key.AccountId,
                group.Key.Code,
                group.Key.NameAr,
                group.Sum(line => line.Debit),
                group.Sum(line => line.Credit),
                group.Sum(line => line.Debit - line.Credit)))
            .OrderBy(line => line.AccountCode)
            .ToListAsync(cancellationToken);

        var cashIn = accounts.Sum(account => account.CashIn);
        var cashOut = accounts.Sum(account => account.CashOut);

        return new CashFlowSummaryReportDto(
            request.FromDate,
            request.ToDate,
            cashIn,
            cashOut,
            cashIn - cashOut,
            accounts);
    }
}
