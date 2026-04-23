using Finance.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finance.Infrastructure.Persistence.Configurations;

public class BusinessTransactionConfiguration : IEntityTypeConfiguration<BusinessTransaction>
{
    public void Configure(EntityTypeBuilder<BusinessTransaction> builder)
    {
        builder.ToTable("Transactions", DatabaseSchemas.Business, tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Transactions_Amount_Positive",
                "\"Amount\" > 0");

            tableBuilder.HasCheckConstraint(
                "CK_Transactions_Different_Accounts",
                "\"DebitAccountId\" <> \"CreditAccountId\"");

            tableBuilder.HasCheckConstraint(
                "CK_Transactions_Completed_Has_Journal",
                "(\"Status\" <> 2) OR (\"JournalId\" IS NOT NULL)");
        });

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Id)
            .ValueGeneratedOnAdd();

        builder.Property(transaction => transaction.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(transaction => transaction.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(transaction => transaction.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(transaction => transaction.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(transaction => transaction.Description)
            .HasMaxLength(BusinessTransaction.MaxDescriptionLength)
            .IsRequired();

        builder.Property(transaction => transaction.ReferenceId)
            .HasMaxLength(BusinessTransaction.MaxReferenceIdLength)
            .IsRequired(false);

        builder.Property(transaction => transaction.CreatedBy)
            .HasMaxLength(BusinessTransaction.MaxCreatedByLength)
            .IsRequired();

        builder.Property(transaction => transaction.CreatedAt)
            .IsRequired();

        builder.Property(transaction => transaction.CompletedAt)
            .IsRequired(false);

        builder.Property(transaction => transaction.FailedAt)
            .IsRequired(false);

        builder.Property(transaction => transaction.FailureReason)
            .HasMaxLength(BusinessTransaction.MaxFailureReasonLength)
            .IsRequired(false);

        builder.HasIndex(transaction => transaction.Type);
        builder.HasIndex(transaction => transaction.Status);
        builder.HasIndex(transaction => transaction.Date);
        builder.HasIndex(transaction => transaction.ReferenceId);
        builder.HasIndex(transaction => transaction.JournalId)
            .IsUnique();

        builder.HasOne(transaction => transaction.DebitAccount)
            .WithMany()
            .HasForeignKey(transaction => transaction.DebitAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(transaction => transaction.CreditAccount)
            .WithMany()
            .HasForeignKey(transaction => transaction.CreditAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(transaction => transaction.Journal)
            .WithMany()
            .HasForeignKey(transaction => transaction.JournalId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
