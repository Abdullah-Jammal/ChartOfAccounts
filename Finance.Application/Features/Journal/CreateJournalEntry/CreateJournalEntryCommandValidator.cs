using Finance.Application.Common.Interfaces.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Journal.CreateJournalEntry;

public class CreateJournalEntryCommandValidator : AbstractValidator<CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator(IApplicationDbContext dbContext)
    {
        RuleFor(command => command.Date)
            .Must(date => date != default)
            .WithMessage("Journal entry date is required.");

        RuleFor(command => command.Lines)
            .NotEmpty()
            .Must(lines => lines.Count >= 2)
            .WithMessage("Journal entry must contain at least two lines.");

        RuleForEach(command => command.Lines)
            .ChildRules(line =>
            {
                line.RuleFor(value => value.AccountId)
                    .GreaterThan(0);

                line.RuleFor(value => value.Debit)
                    .GreaterThanOrEqualTo(0);

                line.RuleFor(value => value.Credit)
                    .GreaterThanOrEqualTo(0);

                line.RuleFor(value => value)
                    .Must(value => value.Debit > 0 ^ value.Credit > 0)
                    .WithMessage("Each journal line must have either debit or credit, but not both.");
            });

        RuleFor(command => command)
            .CustomAsync(async (command, context, cancellationToken) =>
            {
                if (command.Lines is null || command.Lines.Count == 0)
                {
                    return;
                }

                var totalDebit = command.Lines.Sum(line => line.Debit);
                var totalCredit = command.Lines.Sum(line => line.Credit);

                if (totalDebit != totalCredit)
                {
                    context.AddFailure(nameof(command.Lines), "Total debit must equal total credit.");
                }

                var accountIds = command.Lines
                    .Select(line => line.AccountId)
                    .Distinct()
                    .ToArray();

                var accounts = await dbContext.Accounts
                    .AsNoTracking()
                    .Where(account => accountIds.Contains(account.Id))
                    .Select(account => new
                    {
                        account.Id,
                        account.Code,
                        account.IsPosting,
                        account.IsActive
                    })
                    .ToListAsync(cancellationToken);

                var foundAccountIds = accounts
                    .Select(account => account.Id)
                    .ToHashSet();

                foreach (var missingAccountId in accountIds.Where(accountId => !foundAccountIds.Contains(accountId)))
                {
                    context.AddFailure(nameof(command.Lines), $"Account {missingAccountId} was not found.");
                }

                foreach (var account in accounts.Where(account => !account.IsPosting))
                {
                    context.AddFailure(nameof(command.Lines), $"Cannot post to non-posting account {account.Code}.");
                }

                foreach (var account in accounts.Where(account => !account.IsActive))
                {
                    context.AddFailure(nameof(command.Lines), $"Cannot post to inactive account {account.Code}.");
                }
            });
    }
}
