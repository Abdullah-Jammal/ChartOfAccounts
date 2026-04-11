namespace Finance.Infrastructure.Persistence.Initialisation;

public interface IApplicationDbInitializer
{
    Task InitialiseAsync(CancellationToken cancellationToken = default);
}
