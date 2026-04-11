using Finance.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finance.Infrastructure.Persistence.Configurations;

public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines", DatabaseSchemas.Accounting);

        builder.HasKey(line => line.Id);

        builder.Property(line => line.Id)
            .ValueGeneratedOnAdd();

        builder.Property(line => line.Debit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(line => line.Credit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(line => line.AccountId);
        builder.HasIndex(line => line.JournalEntryId);

        builder.HasOne(line => line.Account)
            .WithMany()
            .HasForeignKey(line => line.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
