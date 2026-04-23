using Finance.Application.Common.Interfaces.Accounting;
using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Common.Interfaces.User;
using Finance.Application.Features.Transactions.Dtos;
using Finance.Domain.Transactions;

namespace Finance.Application.Features.Transactions.Common;

internal sealed class BusinessTransactionCommandHandler(
    IApplicationDbContext dbContext,
    IJournalGenerator journalGenerator,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider)
{
    public Task<BusinessTransactionDto> CreateAsync(
        BusinessTransactionType type,
        decimal amount,
        DateOnly date,
        string description,
        string? referenceId,
        int debitAccountId,
        int creditAccountId,
        CancellationToken cancellationToken)
    {
        return dbContext.ExecuteInTransactionAsync(
            async transactionCancellationToken =>
            {
                var now = timeProvider.GetUtcNow().UtcDateTime;
                var createdBy = currentUserService.Email
                    ?? currentUserService.UserId
                    ?? "system";

                var transaction = BusinessTransaction.CreatePending(
                    type,
                    amount,
                    date,
                    description,
                    referenceId,
                    debitAccountId,
                    creditAccountId,
                    createdBy,
                    now);

                dbContext.BusinessTransactions.Add(transaction);
                await dbContext.SaveChangesAsync(transactionCancellationToken);

                var journal = await journalGenerator.GenerateAndPostAsync(
                    transaction,
                    transactionCancellationToken);

                transaction.Complete(journal, timeProvider.GetUtcNow().UtcDateTime);
                await dbContext.SaveChangesAsync(transactionCancellationToken);

                return transaction.ToDto();
            },
            cancellationToken);
    }
}
