using FluentValidation;

namespace Finance.Application.Features.Transactions.GetTransactionById;

public class GetTransactionByIdQueryValidator : AbstractValidator<GetTransactionByIdQuery>
{
    public GetTransactionByIdQueryValidator()
    {
        RuleFor(query => query.TransactionId)
            .GreaterThan(0);
    }
}
