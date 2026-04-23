namespace Finance.Application.Features.Reports.Dtos;

public record TrialBalanceLineDto(
    int AccountId,
    int AccountCode,
    string AccountName,
    string AccountType,
    decimal DebitTotal,
    decimal CreditTotal);
