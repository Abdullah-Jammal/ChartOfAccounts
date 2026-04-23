using Finance.Application.Common.Interfaces.Accounting;
using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Common.Interfaces.User;
using Finance.Application.Features.Transactions.Common;
using Finance.Application.Features.Transactions.Dtos;
using Finance.Domain.Transactions;
using MediatR;

namespace Finance.Application.Features.Transactions.CreateTransferTransaction;

public record CreateTransferTransactionCommand(
    decimal Amount,
    DateOnly Date,
    string Description,
    string? ReferenceId,
    int SourceAccountId,
    int DestinationAccountId) : IRequest<BusinessTransactionDto>;

public class CreateTransferTransactionCommandHandler(
    IApplicationDbContext dbContext,
    IJournalGenerator journalGenerator,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider) : IRequestHandler<CreateTransferTransactionCommand, BusinessTransactionDto>
{
    public Task<BusinessTransactionDto> Handle(
        CreateTransferTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var handler = new BusinessTransactionCommandHandler(
            dbContext,
            journalGenerator,
            currentUserService,
            timeProvider);

        return handler.CreateAsync(
            BusinessTransactionType.Transfer,
            request.Amount,
            request.Date,
            request.Description,
            request.ReferenceId,
            request.DestinationAccountId,
            request.SourceAccountId,
            cancellationToken);
    }
}
