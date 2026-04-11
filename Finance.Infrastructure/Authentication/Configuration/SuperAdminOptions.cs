using Finance.Application.Common.Constants;

namespace Finance.Infrastructure.Authentication.Configuration;

public class SuperAdminOptions
{
    public const string SectionName = "SuperAdmin";

    public string Email { get; set; } = "superadmin@finance.local";
    public string Password { get; set; } = "SuperAdmin123!";
    public string FullName { get; set; } = "Super Admin";
    public string Role { get; set; } = Roles.SuperAdmin;
}
