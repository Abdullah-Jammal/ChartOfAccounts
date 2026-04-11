using Finance.Application.Common.Interfaces.Auth;
using Finance.Application.Common.Interfaces.Persistence;
using Finance.Infrastructure.Authentication.Configuration;
using Finance.Infrastructure.Authentication.Services;
using Finance.Infrastructure.Identity;
using Finance.Infrastructure.Identity.Entity;
using Finance.Infrastructure.Identity.Services;
using Finance.Infrastructure.Persistence.Initialisation;
using Finance.Infrastructure.Persistence.SeedData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Finance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var issuer = jwtSection[nameof(JwtOptions.Issuer)] ?? throw new InvalidOperationException("JWT issuer is not configured.");
        var audience = jwtSection[nameof(JwtOptions.Audience)] ?? throw new InvalidOperationException("JWT audience is not configured.");
        var secretKey = jwtSection[nameof(JwtOptions.SecretKey)] ?? throw new InvalidOperationException("JWT secret key is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<JwtOptions>(jwtSection);
        services.Configure<SuperAdminOptions>(configuration.GetSection(SuperAdminOptions.SectionName));

        services.AddSingleton(TimeProvider.System);

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IApplicationDbInitializer, ApplicationDbInitializer>();
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        services.AddScoped<IChartOfAccountsSeeder, ChartOfAccountsSeeder>();

        return services;
    }
}
