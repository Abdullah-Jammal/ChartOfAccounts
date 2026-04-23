using Finance.Application.Features.Reports.Common;
using FluentValidation;

namespace Finance.Application.Features.Reports.GetCashFlow;

public class GetCashFlowQueryValidator : AbstractValidator<GetCashFlowQuery>
{
    public GetCashFlowQueryValidator()
    {
        this.AddDateRangeRule(
            query => query.FromDate,
            query => query.ToDate);
    }
}
