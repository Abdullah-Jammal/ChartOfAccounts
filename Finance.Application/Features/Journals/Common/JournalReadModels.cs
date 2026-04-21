using Finance.Application.Common.Exceptions;
using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Journals.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Journals.Common;

internal static class JournalReadModels
{
    public static async Task<JournalDto> GetJournalDtoAsync(
        IApplicationDbContext dbContext,
        int journalId,
        CancellationToken cancellationToken)
    {
        var journal = await dbContext.Journals
            .AsNoTracking()
            .Include(entry => entry.Lines)
            .ThenInclude(line => line.Account)
            .SingleOrDefaultAsync(entry => entry.Id == journalId, cancellationToken);

        if (journal is null)
        {
            throw new NotFoundException($"Journal {journalId} was not found.");
        }

        return journal.ToDto();
    }
}
