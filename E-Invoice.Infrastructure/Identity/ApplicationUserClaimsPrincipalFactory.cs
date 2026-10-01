using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using EInvoice.Infrastructure.Authorization;

namespace EInvoice.Infrastructure.Identity;

/// <summary>
/// Adds each role's permission claims to the user's principal at sign-in time, so
/// authorization policy checks are a cheap claim lookup rather than a database hit.
/// A role's permission changes take effect for a user the next time they sign in
/// (or on the periodic security-stamp revalidation already configured for Blazor).
/// </summary>
public class ApplicationUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;

        identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
        identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));

        var roleNames = await UserManager.GetRolesAsync(user);
        var grantedPermissions = new HashSet<string>(StringComparer.Ordinal);

        foreach (var roleName in roleNames)
        {
            var role = await RoleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                continue;
            }

            var roleClaims = await RoleManager.GetClaimsAsync(role);
            foreach (var claim in roleClaims.Where(c => c.Type == PermissionClaimTypes.Permission))
            {
                grantedPermissions.Add(claim.Value);
            }
        }

        foreach (var permission in grantedPermissions)
        {
            identity.AddClaim(new Claim(PermissionClaimTypes.Permission, permission));
        }

        return principal;
    }
}
