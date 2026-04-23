using Finance.Domain.Accounting;
using Finance.Domain.Transactions;
using FluentValidation;
using System.Linq.Expressions;

namespace Finance.Application.Features.Transactions.Common;

internal static class BusinessTransactionCommandValidator
{
    public static void AddCommonRules<T>(
        this AbstractValidator<T> validator,
        Expression<Func<T, decimal>> amountSelector,
        Expression<Func<T, DateOnly>> dateSelector,
        Expression<Func<T, string>> descriptionSelector,
        Expression<Func<T, string?>> referenceIdSelector)
    {
        var referenceIdAccessor = referenceIdSelector.Compile();

        validator.RuleFor(amountSelector)
            .GreaterThan(0)
            .Must(amount => decimal.Round(amount, 2, MidpointRounding.AwayFromZero) == amount)
            .WithMessage("Amount cannot have more than 2 decimal places.");

        validator.RuleFor(dateSelector)
            .Must(date => date != default)
            .WithMessage("Transaction date is required.");

        validator.RuleFor(descriptionSelector)
            .NotEmpty()
            .MaximumLength(Journal.MaxDescriptionLength);

        validator.RuleFor(referenceIdSelector)
            .MaximumLength(BusinessTransaction.MaxReferenceIdLength)
            .When(command => !string.IsNullOrWhiteSpace(referenceIdAccessor(command)));
    }
}
