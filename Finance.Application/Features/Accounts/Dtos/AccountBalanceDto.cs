namespace Finance.Application.Features.Accounts.Dtos;

public record AccountBalanceDto(
    int AccountId,
    int AccountCode,
    string AccountName,
    string AccountType,
    DateOnly? AsOfDate,
    decimal DebitTotal,
    decimal CreditTotal,
    decimal Balance,
    bool IncludesChildAccounts);
