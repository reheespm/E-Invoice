using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EInvoice.Domain.Authorization;
using EInvoice.Infrastructure.Authorization;
using EInvoice.Infrastructure.Identity;

namespace EInvoice.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await SeedRolesAsync(roleManager);
        await SeedAdministratorAsync(userManager, configuration, logger);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        var allPermissions = Permissions.GetAllValues();

        foreach (var roleName in Roles.All)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                role = new ApplicationRole(roleName, $"Built-in {roleName} role");
                await roleManager.CreateAsync(role);
            }

            if (roleName == Roles.Administrator)
            {
                var existingClaims = (await roleManager.GetClaimsAsync(role))
                    .Where(c => c.Type == PermissionClaimTypes.Permission)
                    .Select(c => c.Value)
                    .ToHashSet(StringComparer.Ordinal);

                foreach (var permission in allPermissions.Where(p => !existingClaims.Contains(p)))
                {
                    await roleManager.AddClaimAsync(role, new Claim(PermissionClaimTypes.Permission, permission));
                }
            }
        }

        // Manager gets read-only visibility by default; adjust from the Roles admin screen later.
        var managerRole = await roleManager.FindByNameAsync(Roles.Manager);
        if (managerRole is not null)
        {
            var existingClaims = (await roleManager.GetClaimsAsync(managerRole))
                .Where(c => c.Type == PermissionClaimTypes.Permission)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.Ordinal);

            var defaultManagerPermissions = new[] { Permissions.Dashboard.View };
            foreach (var permission in defaultManagerPermissions.Where(p => !existingClaims.Contains(p)))
            {
                await roleManager.AddClaimAsync(managerRole, new Claim(PermissionClaimTypes.Permission, permission));
            }
        }

        var userRole = await roleManager.FindByNameAsync(Roles.User);
        if (userRole is not null)
        {
            var existingClaims = (await roleManager.GetClaimsAsync(userRole))
                .Where(c => c.Type == PermissionClaimTypes.Permission)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.Ordinal);

            if (!existingClaims.Contains(Permissions.Dashboard.View))
            {
                await roleManager.AddClaimAsync(userRole, new Claim(PermissionClaimTypes.Permission, Permissions.Dashboard.View));
            }
        }
    }

    private static async Task SeedAdministratorAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@einvoice.local";
        var adminPassword = configuration["Seed:AdminPassword"] ?? "Admin#12345";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            IsActive = true,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (!result.Succeeded)
        {
            logger.LogError(
                "Failed to seed the administrator account: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(admin, Roles.Administrator);
        logger.LogInformation("Seeded initial administrator account {Email}", adminEmail);
    }
}
