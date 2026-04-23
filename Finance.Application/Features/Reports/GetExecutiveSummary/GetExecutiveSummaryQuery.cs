using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Reports.Common;
using Finance.Application.Features.Reports.Dtos;
using Finance.Domain.Accounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Reports.GetExecutiveSummary;

public record GetExecutiveSummaryQuery(DateOnly? FromDate, DateOnly? ToDate)
    : IRequest<ExecutiveSummaryReportDto>;

public class GetExecutiveSummaryQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetExecutiveSummaryQuery, ExecutiveSummaryReportDto>
{
    public async Task<ExecutiveSummaryReportDto> Handle(
        GetExecutiveSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var periodSummaries = await PostedAccountMovementReader.GetSummariesAsync(
            dbContext,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        var asOfSummaries = await PostedAccountMovementReader.GetSummariesAsync(
            dbContext,
            null,
            request.ToDate,
            cancellationToken);

        var totalRevenue = periodSummaries
            .Where(summary => summary.AccountType == AccountType.Revenue)
            .Sum(summary => summary.CreditMinusDebit);

        var totalExpenses = periodSummaries
            .Where(summary => summary.AccountType == AccountType.Expense)
            .Sum(summary => summary.DebitMinusCredit);

        var totalAssets = asOfSummaries
            .Where(summary => summary.AccountType == AccountType.Asset)
            .Sum(summary => summary.DebitMinusCredit);

        var totalLiabilities = asOfSummaries
            .Where(summary => summary.AccountType == AccountType.Liability)
            .Sum(summary => summary.CreditMinusDebit);

        var cashAccountIds = await CashAccountReader.GetCashAccountIdsAsync(dbContext, cancellationToken);
        var cashQuery = dbContext.JournalLines
            .AsNoTracking()
            .Where(line => cashAccountIds.Contains(line.AccountId) &&
                line.Journal.Status == JournalStatus.Posted);

        if (request.ToDate is not null)
        {
            cashQuery = cashQuery.Where(line => line.Journal.Date <= request.ToDate.Value);
        }

        var cashBalance = await cashQuery
            .SumAsync(line => line.Debit - line.Credit, cancellationToken);

        return new ExecutiveSummaryReportDto(
            request.FromDate,
            request.ToDate,
            totalRevenue,
            totalExpenses,
            totalRevenue - totalExpenses,
            cashBalance,
            totalAssets,
            totalLiabilities);
    }
}
