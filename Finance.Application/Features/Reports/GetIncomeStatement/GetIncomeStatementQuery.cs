using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Reports.Common;
using Finance.Application.Features.Reports.Dtos;
using Finance.Domain.Accounting;
using MediatR;

namespace Finance.Application.Features.Reports.GetIncomeStatement;

public record GetIncomeStatementQuery(DateOnly? FromDate, DateOnly? ToDate)
    : IRequest<IncomeStatementReportDto>;

public class GetIncomeStatementQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetIncomeStatementQuery, IncomeStatementReportDto>
{
    public async Task<IncomeStatementReportDto> Handle(
        GetIncomeStatementQuery request,
        CancellationToken cancellationToken)
    {
        var summaries = await PostedAccountMovementReader.GetSummariesAsync(
            dbContext,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        var revenues = summaries
            .Where(summary => summary.AccountType == AccountType.Revenue)
            .Select(summary => new IncomeStatementLineDto(
                summary.AccountId,
                summary.AccountCode,
                summary.AccountName,
                summary.CreditMinusDebit))
            .Where(line => line.Amount != 0m)
            .ToList();

        var expenses = summaries
            .Where(summary => summary.AccountType == AccountType.Expense)
            .Select(summary => new IncomeStatementLineDto(
                summary.AccountId,
                summary.AccountCode,
                summary.AccountName,
                summary.DebitMinusCredit))
            .Where(line => line.Amount != 0m)
            .ToList();

        var totalRevenues = revenues.Sum(line => line.Amount);
        var totalExpenses = expenses.Sum(line => line.Amount);

        return new IncomeStatementReportDto(
            request.FromDate,
            request.ToDate,
            totalRevenues,
            totalExpenses,
            totalRevenues - totalExpenses,
            revenues,
            expenses);
    }
}
