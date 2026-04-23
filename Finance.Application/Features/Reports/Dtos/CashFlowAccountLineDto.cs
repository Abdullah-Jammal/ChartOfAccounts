namespace Finance.Application.Features.Reports.Dtos;

public record CashFlowAccountLineDto(
    int AccountId,
    int AccountCode,
    string AccountName,
    decimal CashIn,
    decimal CashOut,
    decimal NetCash);
