using Finance.Application.Common.Interfaces.Accounting;
using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Common.Interfaces.User;
using Finance.Application.Features.Transactions.Common;
using Finance.Application.Features.Transactions.Dtos;
using Finance.Domain.Transactions;
using MediatR;

namespace Finance.Application.Features.Transactions.CreatePaymentTransaction;

public record CreatePaymentTransactionCommand(
    decimal Amount,
    DateOnly Date,
    string Description,
    string? ReferenceId,
    int CashAccountId,
    int RevenueAccountId) : IRequest<BusinessTransactionDto>;

public class CreatePaymentTransactionCommandHandler(
    IApplicationDbContext dbContext,
    IJournalGenerator journalGenerator,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider) : IRequestHandler<CreatePaymentTransactionCommand, BusinessTransactionDto>
{
    public Task<BusinessTransactionDto> Handle(
        CreatePaymentTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var handler = new BusinessTransactionCommandHandler(
            dbContext,
            journalGenerator,
            currentUserService,
            timeProvider);

        return handler.CreateAsync(
            BusinessTransactionType.Payment,
            request.Amount,
            request.Date,
            request.Description,
            request.ReferenceId,
            request.CashAccountId,
            request.RevenueAccountId,
            cancellationToken);
    }
}
