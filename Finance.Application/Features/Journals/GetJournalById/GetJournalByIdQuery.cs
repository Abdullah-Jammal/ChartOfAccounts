using Finance.Application.Common.Interfaces.Persistence;
using Finance.Application.Features.Journals.Common;
using Finance.Application.Features.Journals.Dtos;
using MediatR;

namespace Finance.Application.Features.Journals.GetJournalById;

public record GetJournalByIdQuery(int JournalId) : IRequest<JournalDto>;

public class GetJournalByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetJournalByIdQuery, JournalDto>
{
    public Task<JournalDto> Handle(GetJournalByIdQuery request, CancellationToken cancellationToken)
    {
        return JournalReadModels.GetJournalDtoAsync(dbContext, request.JournalId, cancellationToken);
    }
}
