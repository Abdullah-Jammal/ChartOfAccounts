using Finance.Application.Common.Constants;
using Finance.Infrastructure.Authentication.Configuration;
using Finance.Infrastructure.Identity;
using Finance.Infrastructure.Identity.Entity;
using Finance.Infrastructure.Persistence.SeedData;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Finance.Infrastructure.Persistence.Initialisation;

public class ApplicationDbInitializer(
    ApplicationDbContext dbContext,
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IChartOfAccountsSeeder chartOfAccountsSeeder,
    IOptions<SuperAdminOptions> superAdminOptions) : IApplicationDbInitializer
{
    private readonly SuperAdminOptions _superAdminOptions = superAdminOptions.Value;

    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
        await chartOfAccountsSeeder.SeedAsync(cancellationToken);
        await SeedRolesAsync();
        await SeedSuperAdminAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in new[] { Roles.SuperAdmin, Roles.User })
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(role));

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(error => error.Description)));
            }
        }
    }

    private async Task SeedSuperAdminAsync()
    {
        var email = _superAdminOptions.Email.Trim();
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = ApplicationUser.Create(email);
            user.SetFullName(_superAdminOptions.FullName);

            var createResult = await userManager.CreateAsync(user, _superAdminOptions.Password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", createResult.Errors.Select(error => error.Description)));
            }
        }
        else if (!string.IsNullOrWhiteSpace(_superAdminOptions.FullName) &&
                 !string.Equals(user.FullName?.Value, _superAdminOptions.FullName, StringComparison.Ordinal))
        {
            user.SetFullName(_superAdminOptions.FullName);
            await userManager.UpdateAsync(user);
        }

        if (!await userManager.IsInRoleAsync(user, _superAdminOptions.Role))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(user, _superAdminOptions.Role);

            if (!addToRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", addToRoleResult.Errors.Select(error => error.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, Roles.User))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(user, Roles.User);

            if (!addToRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", addToRoleResult.Errors.Select(error => error.Description)));
            }
        }
    }
}
