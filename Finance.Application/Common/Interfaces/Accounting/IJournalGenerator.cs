using Finance.Domain.Accounting;
using Finance.Domain.Transactions;

namespace Finance.Application.Common.Interfaces.Accounting;

public interface IJournalGenerator
{
    Task<Journal> GenerateAndPostAsync(
        BusinessTransaction transaction,
        CancellationToken cancellationToken = default);
}
