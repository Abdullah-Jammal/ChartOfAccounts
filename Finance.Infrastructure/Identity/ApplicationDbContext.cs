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
    public DbSet<Journal> Journals => Set<Journal>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();
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

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
