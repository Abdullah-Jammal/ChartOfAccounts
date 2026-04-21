using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Journals.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finance.Application.Features.Journals.GetJournalsList;

public record GetJournalsListQuery : IRequest<IReadOnlyList<JournalSummaryDto>>;

public class GetJournalsListQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetJournalsListQuery, IReadOnlyList<JournalSummaryDto>>
{
    public async Task<IReadOnlyList<JournalSummaryDto>> Handle(
        GetJournalsListQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.Journals
            .AsNoTracking()
            .OrderByDescending(journal => journal.Date)
            .ThenByDescending(journal => journal.Id)
            .Select(journal => new JournalSummaryDto(
                journal.Id,
                journal.Date,
                journal.Description,
                journal.ReferenceNumber,
                journal.Status,
                journal.CreatedAt,
                journal.CreatedBy,
                journal.Lines.Sum(line => line.Debit),
                journal.Lines.Sum(line => line.Credit),
                journal.Lines.Count))
            .ToListAsync(cancellationToken);
    }
}
