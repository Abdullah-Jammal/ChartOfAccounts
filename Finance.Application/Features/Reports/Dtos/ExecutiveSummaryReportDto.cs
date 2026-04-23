namespace Finance.Application.Features.Reports.Dtos;

public record ExecutiveSummaryReportDto(
    DateOnly? FromDate,
    DateOnly? ToDate,
    decimal TotalRevenue,
    decimal TotalExpenses,
    decimal NetProfit,
    decimal CashBalance,
    decimal TotalAssets,
    decimal TotalLiabilities);
