namespace Finance.Infrastructure.Persistence.SeedData;

public interface IChartOfAccountsSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
