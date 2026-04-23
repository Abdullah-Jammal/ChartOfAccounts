using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Reports.Common;
using Finance.Application.Features.Reports.Dtos;
using MediatR;

namespace Finance.Application.Features.Reports.GetTrialBalance;

public record GetTrialBalanceQuery(DateOnly? FromDate, DateOnly? ToDate)
    : IRequest<TrialBalanceReportDto>;

public class GetTrialBalanceQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTrialBalanceQuery, TrialBalanceReportDto>
{
    public async Task<TrialBalanceReportDto> Handle(
        GetTrialBalanceQuery request,
        CancellationToken cancellationToken)
    {
        var summaries = await PostedAccountMovementReader.GetSummariesAsync(
            dbContext,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        var lines = summaries
            .Where(summary => summary.DebitMinusCredit != 0m)
            .Select(summary =>
            {
                var balance = summary.DebitMinusCredit;

                return new TrialBalanceLineDto(
                    summary.AccountId,
                    summary.AccountCode,
                    summary.AccountName,
                    summary.AccountType.ToString(),
                    balance > 0m ? balance : 0m,
                    balance < 0m ? -balance : 0m);
            })
            .ToList();

        var totalDebit = lines.Sum(line => line.DebitTotal);
        var totalCredit = lines.Sum(line => line.CreditTotal);

        return new TrialBalanceReportDto(
            request.FromDate,
            request.ToDate,
            totalDebit,
            totalCredit,
            totalDebit == totalCredit,
            lines);
    }
}
