using Finance.Domain.Transactions;

namespace Finance.Application.Features.Transactions.Dtos;

public static class BusinessTransactionMappings
{
    public static BusinessTransactionDto ToDto(this BusinessTransaction transaction)
    {
        return new BusinessTransactionDto(
            transaction.Id,
            transaction.Type,
            transaction.Amount,
            transaction.Date,
            transaction.Description,
            transaction.ReferenceId,
            transaction.Status,
            transaction.CreatedBy,
            transaction.CreatedAt,
            transaction.CompletedAt,
            transaction.FailedAt,
            transaction.FailureReason,
            transaction.DebitAccountId,
            transaction.CreditAccountId,
            transaction.JournalId);
    }
}
