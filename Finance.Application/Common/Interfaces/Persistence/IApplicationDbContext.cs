using Finance.Domain.Accounting;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Common.Interfaces.Persistence;

public interface IApplicationDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<JournalEntryLine> JournalEntryLines { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
