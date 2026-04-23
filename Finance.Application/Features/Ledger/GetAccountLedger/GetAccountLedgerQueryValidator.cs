using FluentValidation;

namespace Finance.Application.Features.Ledger.GetAccountLedger;

public class GetAccountLedgerQueryValidator : AbstractValidator<GetAccountLedgerQuery>
{
    public GetAccountLedgerQueryValidator()
    {
        RuleFor(query => query.AccountId)
            .GreaterThan(0);

        RuleFor(query => query)
            .Must(query => query.FromDate is null || query.ToDate is null || query.FromDate <= query.ToDate)
            .WithMessage("From date must be earlier than or equal to to date.");

        RuleFor(query => query.PageNumber)
            .GreaterThan(0)
            .When(query => query.PageNumber is not null);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 500)
            .When(query => query.PageSize is not null);

        RuleFor(query => query)
            .Must(query => (query.PageNumber is null && query.PageSize is null) ||
                (query.PageNumber is not null && query.PageSize is not null))
            .WithMessage("Page number and page size must be provided together.");
    }
}
