namespace Finance.Application.Features.Reports.Dtos;

public record CashFlowSummaryReportDto(
    DateOnly? FromDate,
    DateOnly? ToDate,
    decimal CashIn,
    decimal CashOut,
    decimal NetCash,
    IReadOnlyList<CashFlowAccountLineDto> Accounts);
