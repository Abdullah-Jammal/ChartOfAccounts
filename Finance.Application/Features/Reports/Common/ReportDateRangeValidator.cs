using FluentValidation;

namespace Finance.Application.Features.Reports.Common;

internal static class ReportDateRangeValidator
{
    public static void AddDateRangeRule<T>(
        this AbstractValidator<T> validator,
        Func<T, DateOnly?> fromDateSelector,
        Func<T, DateOnly?> toDateSelector)
    {
        validator.RuleFor(query => query)
            .Must(query =>
            {
                var fromDate = fromDateSelector(query);
                var toDate = toDateSelector(query);

                return fromDate is null || toDate is null || fromDate <= toDate;
            })
            .WithMessage("From date must be earlier than or equal to to date.");
    }
}
