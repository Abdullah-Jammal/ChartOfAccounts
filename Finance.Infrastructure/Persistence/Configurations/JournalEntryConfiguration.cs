using Finance.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finance.Infrastructure.Persistence.Configurations;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries", DatabaseSchemas.Accounting);

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .ValueGeneratedOnAdd();

        builder.Property(entry => entry.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.HasMany(entry => entry.Lines)
            .WithOne(line => line.JournalEntry)
            .HasForeignKey(line => line.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(entry => entry.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
