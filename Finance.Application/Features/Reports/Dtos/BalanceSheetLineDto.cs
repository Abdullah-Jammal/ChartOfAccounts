namespace Finance.Application.Features.Reports.Dtos;

public record BalanceSheetLineDto(
    int AccountId,
    int AccountCode,
    string AccountName,
    decimal Amount);
