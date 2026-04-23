using FluentValidation;

namespace Finance.Application.Features.Transactions.GetTransactionsList;

public class GetTransactionsListQueryValidator : AbstractValidator<GetTransactionsListQuery>
{
    public GetTransactionsListQueryValidator()
    {
        RuleFor(query => query)
            .Must(query => query.FromDate is null || query.ToDate is null || query.FromDate <= query.ToDate)
            .WithMessage("From date must be earlier than or equal to to date.");

        RuleFor(query => query.PageNumber)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 500);
    }
}
