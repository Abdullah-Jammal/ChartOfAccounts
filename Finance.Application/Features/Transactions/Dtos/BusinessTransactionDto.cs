using Finance.Domain.Transactions;

namespace Finance.Application.Features.Transactions.Dtos;

public record BusinessTransactionDto(
    int Id,
    BusinessTransactionType Type,
    decimal Amount,
    DateOnly Date,
    string Description,
    string? ReferenceId,
    BusinessTransactionStatus Status,
    string CreatedBy,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    DateTime? FailedAt,
    string? FailureReason,
    int DebitAccountId,
    int CreditAccountId,
    int? JournalId);
