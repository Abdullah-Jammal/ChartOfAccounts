namespace Finance.Application.Features.Reports.Dtos;

public record BalanceSheetReportDto(
    DateOnly? AsOfDate,
    decimal TotalAssets,
    decimal TotalLiabilities,
    decimal TotalEquity,
    bool IsBalanced,
    IReadOnlyList<BalanceSheetLineDto> Assets,
    IReadOnlyList<BalanceSheetLineDto> Liabilities,
    IReadOnlyList<EquityLineDto> Equity);
