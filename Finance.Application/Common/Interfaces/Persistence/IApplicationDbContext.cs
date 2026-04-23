using Finance.Domain.Accounting;
using Finance.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Common.Interfaces.Persistence;

public interface IApplicationDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<Journal> Journals { get; }
    DbSet<JournalLine> JournalLines { get; }
    DbSet<BusinessTransaction> BusinessTransactions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);
}
