namespace Finance.Application.Features.Reports.Dtos;

public record TrialBalanceReportDto(
    DateOnly? FromDate,
    DateOnly? ToDate,
    decimal TotalDebit,
    decimal TotalCredit,
    bool IsBalanced,
    IReadOnlyList<TrialBalanceLineDto> Accounts);
