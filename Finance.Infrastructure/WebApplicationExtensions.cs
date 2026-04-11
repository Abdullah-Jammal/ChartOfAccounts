using Finance.Infrastructure.Persistence.Initialisation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Finance.Infrastructure;

public static class WebApplicationExtensions
{
    public static async Task<WebApplication> InitialiseInfrastructureAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IApplicationDbInitializer>();
        await initializer.InitialiseAsync();

        return app;
    }
}
