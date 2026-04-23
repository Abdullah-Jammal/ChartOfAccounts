namespace Finance.Application.Features.Reports.Dtos;

public record IncomeStatementLineDto(
    int AccountId,
    int AccountCode,
    string AccountName,
    decimal Amount);
