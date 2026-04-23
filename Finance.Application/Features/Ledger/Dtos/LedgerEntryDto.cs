namespace Finance.Application.Features.Ledger.Dtos;

public record LedgerEntryDto(
    int AccountId,
    string AccountName,
    int JournalId,
    DateOnly Date,
    string Description,
    decimal Debit,
    decimal Credit,
    decimal RunningBalance);
