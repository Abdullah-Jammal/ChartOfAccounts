using Finance.Domain.Accounting;

namespace Finance.Application.Features.Journals.Dtos;

public static class JournalMappings
{
    public static JournalDto ToDto(this Journal journal)
    {
        return new JournalDto(
            journal.Id,
            journal.Date,
            journal.Description,
            journal.ReferenceNumber,
            journal.Status,
            journal.CreatedAt,
            journal.CreatedBy,
            journal.TotalDebit,
            journal.TotalCredit,
            journal.Lines
                .OrderBy(line => line.Id)
                .Select(line => new JournalLineDto(
                    line.Id,
                    line.AccountId,
                    line.Account.Code,
                    line.Account.NameAr,
                    line.Debit,
                    line.Credit))
                .ToList());
    }
}
