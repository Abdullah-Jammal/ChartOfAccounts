using Finance.Domain.Accounting;

namespace Finance.Application.Features.Journals.Dtos;

public record JournalSummaryDto(
    int Id,
    DateOnly Date,
    string Description,
    string? ReferenceNumber,
    JournalStatus Status,
    DateTime CreatedAt,
    string CreatedBy,
    decimal TotalDebit,
    decimal TotalCredit,
    int LineCount);
