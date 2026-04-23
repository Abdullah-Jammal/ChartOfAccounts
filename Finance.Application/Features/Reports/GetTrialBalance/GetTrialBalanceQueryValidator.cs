using Finance.Application.Features.Reports.Common;
using FluentValidation;

namespace Finance.Application.Features.Reports.GetTrialBalance;

public class GetTrialBalanceQueryValidator : AbstractValidator<GetTrialBalanceQuery>
{
    public GetTrialBalanceQueryValidator()
    {
        this.AddDateRangeRule(
            query => query.FromDate,
            query => query.ToDate);
    }
}
