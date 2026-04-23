using FluentValidation;

namespace Finance.Application.Features.Reports.GetBalanceSheet;

public class GetBalanceSheetQueryValidator : AbstractValidator<GetBalanceSheetQuery>
{
    public GetBalanceSheetQueryValidator()
    {
    }
}
