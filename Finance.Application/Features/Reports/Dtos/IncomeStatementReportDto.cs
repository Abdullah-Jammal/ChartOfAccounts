namespace Finance.Application.Features.Reports.Dtos;

public record IncomeStatementReportDto(
    DateOnly? FromDate,
    DateOnly? ToDate,
    decimal TotalRevenues,
    decimal TotalExpenses,
    decimal NetProfit,
    IReadOnlyList<IncomeStatementLineDto> Revenues,
    IReadOnlyList<IncomeStatementLineDto> Expenses);
