using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Accounts.Dtos;
using Finance.Domain.Accounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Accounts.CreateAccount;

public record CreateAccountCommand(
    int Code,
    string NameAr,
    string? NameEn,
    int Level,
    int? ParentCode,
    AccountType Type,
    bool IsPosting,
    bool IsActive = true) : IRequest<AccountDto>;

public class CreateAccountCommandHandler(
    IApplicationDbContext dbContext,
    TimeProvider timeProvider) : IRequestHandler<CreateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var parent = request.ParentCode is null
            ? null
            : await dbContext.Accounts
                .SingleAsync(account => account.Code == request.ParentCode.Value, cancellationToken);

        var account = Account.Create(
            request.Code,
            request.NameAr,
            request.NameEn,
            request.Level,
            parent?.Id,
            request.Type,
            request.IsPosting,
            request.IsActive,
            timeProvider.GetUtcNow().UtcDateTime);

        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AccountDto(
            account.Id,
            account.Code,
            account.NameAr,
            account.NameEn,
            account.Level,
            account.ParentId,
            parent?.Code,
            account.Type.ToString(),
            account.IsPosting,
            account.IsActive);
    }
}
