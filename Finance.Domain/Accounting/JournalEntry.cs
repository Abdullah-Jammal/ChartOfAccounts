namespace Finance.Domain.Accounting;

public class JournalEntry
{
    private readonly List<JournalEntryLine> _lines = [];

    public int Id { get; private set; }
    public DateOnly Date { get; private set; }
    public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

    private JournalEntry()
    {
    }

    public static JournalEntry Create(DateOnly date, IEnumerable<JournalEntryLine> lines)
    {
        var entry = new JournalEntry
        {
            Date = date
        };

        entry._lines.AddRange(lines);
        return entry;
    }
}
