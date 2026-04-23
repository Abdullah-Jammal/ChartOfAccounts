using FluentValidation;

namespace Finance.Application.Features.Accounts.GetAccountBalance;

public class GetAccountBalanceQueryValidator : AbstractValidator<GetAccountBalanceQuery>
{
    public GetAccountBalanceQueryValidator()
    {
        RuleFor(query => query.AccountId)
            .GreaterThan(0);
    }
}
