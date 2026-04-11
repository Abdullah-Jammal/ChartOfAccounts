using FluentValidation;

namespace Finance.Application.Features.Accounts.SearchAccounts;

public class SearchAccountsQueryValidator : AbstractValidator<SearchAccountsQuery>
{
    public SearchAccountsQueryValidator()
    {
        RuleFor(query => query.Query)
            .NotEmpty()
            .MaximumLength(100);
    }
}
