using EInvoice.Application.Cities;
using EInvoice.Application.Companies;
using EInvoice.Application.Countries;
using EInvoice.Application.Customers;
using EInvoice.Application.Dashboard;
using EInvoice.Application.Roles;
using EInvoice.Application.Users;
using EInvoice.Infrastructure.Authorization;
using EInvoice.Infrastructure.Identity;
using EInvoice.Infrastructure.Persistence;
using EInvoice.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EInvoice.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<EInvoiceDbContext>(options => options.UseSqlServer(connectionString));

        // Keep login cookies / anti-forgery keys in the database so users stay signed in across
        // restarts and redeploys, and so several server instances can share them later in the cloud.
        services.AddDataProtection()
            .SetApplicationName("E-Invoice")
            .PersistKeysToDbContext<EInvoiceDbContext>();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<EInvoiceDbContext>()
            .AddSignInManager<ApplicationSignInManager>()
            .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
            .AddDefaultTokenProviders();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<ICompanyManagementService, CompanyManagementService>();
        services.AddScoped<ICustomerManagementService, CustomerManagementService>();
        services.AddScoped<ICountryManagementService, CountryManagementService>();
        services.AddScoped<ICityManagementService, CityManagementService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
