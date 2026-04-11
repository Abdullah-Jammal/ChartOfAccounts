using Finance.Infrastructure.Identity.ObjectValue;
using Microsoft.AspNetCore.Identity;

namespace Finance.Infrastructure.Identity.Entity;

public class ApplicationUser : IdentityUser
{
    public FullName? FullName { get; private set; }
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    private ApplicationUser()
    {
    }

    public void SetFullName(string fullName)
    {
        FullName = new FullName(fullName);
    }

    public static ApplicationUser Create(string email)
    {
        return new ApplicationUser
        {
            UserName = email,
            Email = email
        };
    }
}
