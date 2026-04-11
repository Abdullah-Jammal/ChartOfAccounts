using Finance.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finance.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts", DatabaseSchemas.Accounting);

        builder.HasKey(account => account.Id);

        builder.Property(account => account.Id)
            .ValueGeneratedOnAdd();

        builder.Property(account => account.Code)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(account => account.NameAr)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(account => account.NameEn)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(account => account.Level)
            .IsRequired();

        builder.Property(account => account.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(account => account.IsPosting)
            .IsRequired();

        builder.Property(account => account.IsActive)
            .IsRequired();

        builder.Property(account => account.CreatedAt)
            .IsRequired();

        builder.HasIndex(account => account.Code)
            .IsUnique();

        builder.HasIndex(account => account.ParentId);
        builder.HasIndex(account => account.IsPosting);

        builder.HasMany(account => account.Children)
            .WithOne(account => account.Parent)
            .HasForeignKey(account => account.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(account => account.Children)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
