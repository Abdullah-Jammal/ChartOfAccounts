using Finance.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finance.Infrastructure.Persistence.Configurations;

public class JournalConfiguration : IEntityTypeConfiguration<Journal>
{
    public void Configure(EntityTypeBuilder<Journal> builder)
    {
        builder.ToTable("JournalEntries", DatabaseSchemas.Accounting, tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_JournalEntries_Status",
                "\"Status\" IN (0, 1)");
        });

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .ValueGeneratedOnAdd();

        builder.Property(entry => entry.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(entry => entry.Description)
            .HasMaxLength(Journal.MaxDescriptionLength)
            .IsRequired();

        builder.Property(entry => entry.ReferenceNumber)
            .HasMaxLength(Journal.MaxReferenceNumberLength)
            .IsRequired(false);

        builder.Property(entry => entry.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(entry => entry.CreatedAt)
            .IsRequired();

        builder.Property(entry => entry.CreatedBy)
            .HasMaxLength(Journal.MaxCreatedByLength)
            .IsRequired();

        builder.HasMany(entry => entry.Lines)
            .WithOne(line => line.Journal)
            .HasForeignKey(line => line.JournalId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_JournalEntryLines_JournalEntries_JournalEntryId");

        builder.Navigation(entry => entry.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
