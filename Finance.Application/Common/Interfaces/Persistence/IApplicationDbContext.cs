using Finance.Domain.Accounting;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Common.Interfaces.Persistence;

public interface IApplicationDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<Journal> Journals { get; }
    DbSet<JournalLine> JournalLines { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);
}
