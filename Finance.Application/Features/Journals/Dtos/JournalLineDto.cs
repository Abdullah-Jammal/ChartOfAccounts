namespace Finance.Application.Features.Journals.Dtos;

public record JournalLineDto(
    int Id,
    int AccountId,
    int AccountCode,
    string AccountNameAr,
    decimal Debit,
    decimal Credit);
