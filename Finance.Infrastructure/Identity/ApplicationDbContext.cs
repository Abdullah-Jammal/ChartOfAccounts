using Finance.Application.Common.Interfaces.Persistence;
using Finance.Domain.Accounting;
using Finance.Infrastructure.Identity.Entity;
using Finance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Identity;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("AspNetUsers", DatabaseSchemas.Auth);
        builder.Entity<IdentityRole>().ToTable("AspNetRoles", DatabaseSchemas.Auth);
        builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles", DatabaseSchemas.Auth);
        builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims", DatabaseSchemas.Auth);
        builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins", DatabaseSchemas.Auth);
        builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims", DatabaseSchemas.Auth);
        builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens", DatabaseSchemas.Auth);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FullName)
                .HasConversion(
                    fullName => fullName == null ? null : fullName.Value,
                    value => string.IsNullOrWhiteSpace(value) ? null : new(value))
                .HasColumnName("FullName")
                .IsRequired(false);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens", DatabaseSchemas.Auth);

            entity.HasKey(token => token.Id);

            entity.Property(token => token.TokenHash)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(token => token.UserId)
                .IsRequired();

            entity.Property(token => token.CreatedAtUtc)
                .IsRequired();

            entity.Property(token => token.ExpiresAtUtc)
                .IsRequired();

            entity.HasIndex(token => token.TokenHash)
                .IsUnique();

            entity.HasIndex(token => token.UserId);

            entity.HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
