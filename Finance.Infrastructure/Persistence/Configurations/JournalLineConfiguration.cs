using Finance.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finance.Infrastructure.Persistence.Configurations;

public class JournalLineConfiguration : IEntityTypeConfiguration<JournalLine>
{
    public void Configure(EntityTypeBuilder<JournalLine> builder)
    {
        builder.ToTable("JournalEntryLines", DatabaseSchemas.Accounting, tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_JournalEntryLines_NonNegativeAmounts",
                "\"Debit\" >= 0 AND \"Credit\" >= 0");

            tableBuilder.HasCheckConstraint(
                "CK_JournalEntryLines_SingleSidedAmount",
                "(CASE WHEN \"Debit\" > 0 THEN 1 ELSE 0 END) + (CASE WHEN \"Credit\" > 0 THEN 1 ELSE 0 END) = 1");
        });

        builder.HasKey(line => line.Id);

        builder.Property(line => line.Id)
            .ValueGeneratedOnAdd();

        builder.Property(line => line.JournalId)
            .HasColumnName("JournalEntryId")
            .IsRequired();

        builder.Property(line => line.Debit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(line => line.Credit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(line => line.AccountId);

        builder.HasIndex(line => line.JournalId)
            .HasDatabaseName("IX_JournalEntryLines_JournalEntryId");

        builder.HasOne(line => line.Account)
            .WithMany()
            .HasForeignKey(line => line.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
