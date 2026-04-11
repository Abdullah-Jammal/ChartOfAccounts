namespace Finance.Application.Features.Journal.Dtos;

public record JournalEntryDto(
    int Id,
    DateOnly Date,
    decimal TotalDebit,
    decimal TotalCredit,
    IReadOnlyList<JournalEntryLineDto> Lines);
