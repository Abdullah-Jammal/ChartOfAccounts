namespace Finance.Application.Features.Journal.Dtos;

public record JournalEntryLineDto(
    int Id,
    int AccountId,
    int AccountCode,
    string AccountNameAr,
    decimal Debit,
    decimal Credit);
