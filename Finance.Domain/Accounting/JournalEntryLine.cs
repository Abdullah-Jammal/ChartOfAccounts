namespace Finance.Domain.Accounting;

public class JournalEntryLine
{
    public int Id { get; private set; }
    public int JournalEntryId { get; private set; }
    public JournalEntry JournalEntry { get; private set; } = null!;
    public int AccountId { get; private set; }
    public Account Account { get; private set; } = null!;
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }

    private JournalEntryLine()
    {
    }

    public static JournalEntryLine Create(int accountId, decimal debit, decimal credit)
    {
        return new JournalEntryLine
        {
            AccountId = accountId,
            Debit = debit,
            Credit = credit
        };
    }
}
