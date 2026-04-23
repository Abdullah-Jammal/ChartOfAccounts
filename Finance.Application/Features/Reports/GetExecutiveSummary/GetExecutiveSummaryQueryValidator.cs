using Finance.Application.Features.Reports.Common;
using FluentValidation;

namespace Finance.Application.Features.Reports.GetExecutiveSummary;

public class GetExecutiveSummaryQueryValidator : AbstractValidator<GetExecutiveSummaryQuery>
{
    public GetExecutiveSummaryQueryValidator()
    {
        this.AddDateRangeRule(
            query => query.FromDate,
            query => query.ToDate);
    }
}
