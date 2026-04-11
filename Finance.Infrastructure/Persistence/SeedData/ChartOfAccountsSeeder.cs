using System.Text.Json;
using System.Text.Json.Serialization;
using Finance.Domain.Accounting;
using Finance.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Finance.Infrastructure.Persistence.SeedData;

public class ChartOfAccountsSeeder(
    ApplicationDbContext dbContext,
    IHostEnvironment hostEnvironment,
    TimeProvider timeProvider) : IChartOfAccountsSeeder
{
    private const string SeedFileName = "chart-of-accounts.json";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var seedItems = await ReadSeedItemsAsync(cancellationToken);
        if (seedItems.Count == 0)
        {
            return;
        }

        ValidateSeed(seedItems);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var existingAccounts = await dbContext.Accounts
            .Select(account => new { account.Id, account.Code })
            .ToListAsync(cancellationToken);

        var existingCodes = existingAccounts
            .Select(account => account.Code)
            .ToHashSet();

        if (seedItems.All(item => existingCodes.Contains(item.Code)))
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        var codeToId = existingAccounts.ToDictionary(account => account.Code, account => account.Id);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var levelGroup in seedItems
                     .Where(item => !existingCodes.Contains(item.Code))
                     .OrderBy(item => item.Level)
                     .ThenBy(item => item.Code)
                     .GroupBy(item => item.Level))
        {
            var accounts = new List<Account>();

            foreach (var seedItem in levelGroup)
            {
                int? parentId = null;

                if (seedItem.ParentCode is not null &&
                    !codeToId.TryGetValue(seedItem.ParentCode.Value, out var resolvedParentId))
                {
                    throw new InvalidOperationException(
                        $"Cannot seed account {seedItem.Code}; parent {seedItem.ParentCode} was not found.");
                }

                if (seedItem.ParentCode is not null)
                {
                    parentId = codeToId[seedItem.ParentCode.Value];
                }

                accounts.Add(Account.Create(
                    seedItem.Code,
                    seedItem.NameAr,
                    seedItem.NameEn,
                    seedItem.Level,
                    parentId,
                    seedItem.Type,
                    seedItem.IsPosting,
                    seedItem.IsActive,
                    now));
            }

            dbContext.Accounts.AddRange(accounts);
            await dbContext.SaveChangesAsync(cancellationToken);

            foreach (var account in accounts)
            {
                existingCodes.Add(account.Code);
                codeToId[account.Code] = account.Id;
            }
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<AccountSeedItem>> ReadSeedItemsAsync(CancellationToken cancellationToken)
    {
        var seedPath = ResolveSeedPath();

        await using var stream = File.OpenRead(seedPath);
        var seedItems = await JsonSerializer.DeserializeAsync<List<AccountSeedItem>>(
            stream,
            JsonOptions,
            cancellationToken);

        return seedItems ?? [];
    }

    private string ResolveSeedPath()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Persistence", "SeedData", SeedFileName),
            Path.Combine(hostEnvironment.ContentRootPath, "Persistence", "SeedData", SeedFileName),
            Path.GetFullPath(Path.Combine(
                hostEnvironment.ContentRootPath,
                "..",
                "Finance.Infrastructure",
                "Persistence",
                "SeedData",
                SeedFileName))
        };

        var seedPath = candidates.FirstOrDefault(File.Exists);

        if (seedPath is null)
        {
            throw new FileNotFoundException(
                $"Chart of accounts seed file was not found. Checked: {string.Join(", ", candidates)}",
                SeedFileName);
        }

        return seedPath;
    }

    private static void ValidateSeed(IReadOnlyCollection<AccountSeedItem> seedItems)
    {
        var duplicateCodes = seedItems
            .GroupBy(item => item.Code)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateCodes.Length > 0)
        {
            throw new InvalidOperationException(
                $"Chart of accounts seed contains duplicate codes: {string.Join(", ", duplicateCodes)}.");
        }

        var byCode = seedItems.ToDictionary(item => item.Code);

        foreach (var seedItem in seedItems)
        {
            if (seedItem.Code <= 0)
            {
                throw new InvalidOperationException("Account seed code must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(seedItem.NameAr))
            {
                throw new InvalidOperationException($"Account {seedItem.Code} must have an Arabic name.");
            }

            if (seedItem.Level < 1)
            {
                throw new InvalidOperationException($"Account {seedItem.Code} has an invalid level.");
            }

            var codeText = seedItem.Code.ToString();
            var expectedType = GetTypeFromRootDigit(codeText[0]);

            if (seedItem.Type != expectedType)
            {
                throw new InvalidOperationException(
                    $"Account {seedItem.Code} type must match root account type {expectedType}.");
            }

            if (seedItem.Level == 1)
            {
                if (seedItem.ParentCode is not null)
                {
                    throw new InvalidOperationException($"Root account {seedItem.Code} cannot have a parent.");
                }

                continue;
            }

            if (seedItem.ParentCode is null)
            {
                throw new InvalidOperationException($"Account {seedItem.Code} must have a parent.");
            }

            if (!byCode.TryGetValue(seedItem.ParentCode.Value, out var parent))
            {
                throw new InvalidOperationException(
                    $"Account {seedItem.Code} references missing parent {seedItem.ParentCode}.");
            }

            if (!codeText.StartsWith(seedItem.ParentCode.Value.ToString(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Account {seedItem.Code} code must start with parent code {seedItem.ParentCode}.");
            }

            if (seedItem.Level != parent.Level + 1)
            {
                throw new InvalidOperationException(
                    $"Account {seedItem.Code} level must be parent level plus one.");
            }

            if (seedItem.Type != parent.Type)
            {
                throw new InvalidOperationException(
                    $"Account {seedItem.Code} type must match parent account type.");
            }
        }
    }

    private static AccountType GetTypeFromRootDigit(char rootDigit)
    {
        return rootDigit switch
        {
            '1' => AccountType.Asset,
            '2' => AccountType.Liability,
            '3' => AccountType.Expense,
            '4' => AccountType.Revenue,
            '5' => AccountType.CostCenter,
            '6' => AccountType.CostCenter,
            '7' => AccountType.CostCenter,
            '8' => AccountType.CostCenter,
            '9' => AccountType.CostCenter,

            _ => throw new InvalidOperationException(
                $"Root account {rootDigit} is not active in the current accounting model.")
        };
    }

    private sealed record AccountSeedItem(
        int Code,
        string NameAr,
        string? NameEn,
        int Level,
        int? ParentCode,
        AccountType Type,
        bool IsPosting,
        bool IsActive);
}
