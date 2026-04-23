using Finance.Application.Common.Interfaces.Accounting;
using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Common.Interfaces.User;
using Finance.Application.Features.Transactions.Common;
using Finance.Application.Features.Transactions.Dtos;
using Finance.Domain.Transactions;
using MediatR;

namespace Finance.Application.Features.Transactions.CreateExpenseTransaction;

public record CreateExpenseTransactionCommand(
    decimal Amount,
    DateOnly Date,
    string Description,
    string? ReferenceId,
    int ExpenseAccountId,
    int CashAccountId) : IRequest<BusinessTransactionDto>;

public class CreateExpenseTransactionCommandHandler(
    IApplicationDbContext dbContext,
    IJournalGenerator journalGenerator,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider) : IRequestHandler<CreateExpenseTransactionCommand, BusinessTransactionDto>
{
    public Task<BusinessTransactionDto> Handle(
        CreateExpenseTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var handler = new BusinessTransactionCommandHandler(
            dbContext,
            journalGenerator,
            currentUserService,
            timeProvider);

        return handler.CreateAsync(
            BusinessTransactionType.Expense,
            request.Amount,
            request.Date,
            request.Description,
            request.ReferenceId,
            request.ExpenseAccountId,
            request.CashAccountId,
            cancellationToken);
    }
}
