using Finance.Application.Features.Reports.Common;
using FluentValidation;

namespace Finance.Application.Features.Reports.GetIncomeStatement;

public class GetIncomeStatementQueryValidator : AbstractValidator<GetIncomeStatementQuery>
{
    public GetIncomeStatementQueryValidator()
    {
        this.AddDateRangeRule(
            query => query.FromDate,
            query => query.ToDate);
    }
}
