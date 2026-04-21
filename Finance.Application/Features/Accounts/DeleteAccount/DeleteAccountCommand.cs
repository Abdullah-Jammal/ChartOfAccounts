using Finance.Application.Common.Exceptions;
using Finance.Application.Common.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Accounts.DeleteAccount;

public record DeleteAccountCommand(int Code) : IRequest;

public class DeleteAccountCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts
            .SingleOrDefaultAsync(account => account.Code == request.Code, cancellationToken);

        if (account is null)
        {
            throw new NotFoundException($"Account {request.Code} was not found.");
        }

        var hasChildren = await dbContext.Accounts
            .AnyAsync(child => child.ParentId == account.Id, cancellationToken);

        if (hasChildren)
        {
            throw new ConflictException("Cannot delete an account that has children.");
        }

        var hasJournalLines = await dbContext.JournalLines
            .AnyAsync(line => line.AccountId == account.Id, cancellationToken);

        if (hasJournalLines)
        {
            throw new ConflictException("Cannot delete an account that has journal entries.");
        }

        dbContext.Accounts.Remove(account);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
