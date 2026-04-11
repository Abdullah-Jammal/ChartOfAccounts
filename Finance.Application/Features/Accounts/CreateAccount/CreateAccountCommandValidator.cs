using Finance.Application.Common.Interfaces.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Accounts.CreateAccount;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator(IApplicationDbContext dbContext)
    {
        RuleFor(command => command.Code)
            .GreaterThan(0);

        RuleFor(command => command.NameAr)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(command => command.NameEn)
            .MaximumLength(500)
            .When(command => !string.IsNullOrWhiteSpace(command.NameEn));

        RuleFor(command => command.Level)
            .GreaterThanOrEqualTo(1);

        RuleFor(command => command)
            .CustomAsync(async (command, context, cancellationToken) =>
            {
                if (await dbContext.Accounts.AnyAsync(account => account.Code == command.Code, cancellationToken))
                {
                    context.AddFailure(nameof(command.Code), "Account code must be unique.");
                }

                if (!AccountHierarchyRules.TryGetTypeFromCode(command.Code, out var expectedType))
                {
                    context.AddFailure(nameof(command.Code), "Only account roots 1-4 are active in this module.");
                    return;
                }

                if (command.Type != expectedType)
                {
                    context.AddFailure(nameof(command.Type), "Account type must match the root account code.");
                }

                if (command.Level == 1)
                {
                    if (command.ParentCode is not null)
                    {
                        context.AddFailure(nameof(command.ParentCode), "Root accounts cannot have a parent.");
                    }

                    if (command.Code is < 1 or > 4)
                    {
                        context.AddFailure(nameof(command.Code), "Only root accounts 1-4 are active in this module.");
                    }

                    return;
                }

                if (command.ParentCode is null)
                {
                    context.AddFailure(nameof(command.ParentCode), "Child accounts must have a valid parent.");
                    return;
                }

                var parent = await dbContext.Accounts
                    .AsNoTracking()
                    .SingleOrDefaultAsync(account => account.Code == command.ParentCode.Value, cancellationToken);

                if (parent is null)
                {
                    context.AddFailure(nameof(command.ParentCode), "Parent account was not found.");
                    return;
                }

                if (!command.Code.ToString().StartsWith(parent.Code.ToString(), StringComparison.Ordinal))
                {
                    context.AddFailure(nameof(command.Code), "Child account code must start with the parent code.");
                }

                if (command.Type != parent.Type)
                {
                    context.AddFailure(nameof(command.Type), "Child account type must match parent account type.");
                }

                if (command.Level != parent.Level + 1)
                {
                    context.AddFailure(nameof(command.Level), "Account level must be parent level plus one.");
                }

                if (parent.IsPosting)
                {
                    context.AddFailure(nameof(command.ParentCode), "Cannot create a child under a posting account.");
                }
            });
    }
}
