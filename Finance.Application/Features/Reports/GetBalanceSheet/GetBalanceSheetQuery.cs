using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Reports.Common;
using Finance.Application.Features.Reports.Dtos;
using Finance.Domain.Accounting;
using MediatR;

namespace Finance.Application.Features.Reports.GetBalanceSheet;

public record GetBalanceSheetQuery(DateOnly? AsOfDate) : IRequest<BalanceSheetReportDto>;

public class GetBalanceSheetQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetBalanceSheetQuery, BalanceSheetReportDto>
{
    public async Task<BalanceSheetReportDto> Handle(
        GetBalanceSheetQuery request,
        CancellationToken cancellationToken)
    {
        var summaries = await PostedAccountMovementReader.GetSummariesAsync(
            dbContext,
            null,
            request.AsOfDate,
            cancellationToken);

        var assets = summaries
            .Where(summary => summary.AccountType == AccountType.Asset)
            .Select(summary => new BalanceSheetLineDto(
                summary.AccountId,
                summary.AccountCode,
                summary.AccountName,
                summary.DebitMinusCredit))
            .Where(line => line.Amount != 0m)
            .ToList();

        var liabilities = summaries
            .Where(summary => summary.AccountType == AccountType.Liability)
            .Select(summary => new BalanceSheetLineDto(
                summary.AccountId,
                summary.AccountCode,
                summary.AccountName,
                summary.CreditMinusDebit))
            .Where(line => line.Amount != 0m)
            .ToList();

        var currentEarnings = summaries
            .Where(summary => summary.AccountType is AccountType.Revenue or AccountType.Expense)
            .Sum(summary => summary.AccountType == AccountType.Revenue
                ? summary.CreditMinusDebit
                : -summary.DebitMinusCredit);

        var equity = new List<EquityLineDto>
        {
            new("Current earnings", currentEarnings)
        };

        var totalAssets = assets.Sum(line => line.Amount);
        var totalLiabilities = liabilities.Sum(line => line.Amount);
        var totalEquity = equity.Sum(line => line.Amount);

        return new BalanceSheetReportDto(
            request.AsOfDate,
            totalAssets,
            totalLiabilities,
            totalEquity,
            totalAssets == totalLiabilities + totalEquity,
            assets,
            liabilities,
            equity);
    }
}
