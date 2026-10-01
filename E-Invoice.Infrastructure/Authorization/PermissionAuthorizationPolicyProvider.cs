using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using EInvoice.Domain.Authorization;

namespace EInvoice.Infrastructure.Authorization;

/// <summary>
/// Resolves an authorization policy for any string that matches a known permission
/// (e.g. "User.Create") on demand, so new permissions never require a matching
/// AddPolicy(...) registration in Program.cs.
/// </summary>
public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private static readonly HashSet<string> KnownPermissions = new(Permissions.GetAllValues(), StringComparer.Ordinal);

    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var existing = await base.GetPolicyAsync(policyName);
        if (existing is not null)
        {
            return existing;
        }

        if (!KnownPermissions.Contains(policyName))
        {
            return null;
        }

        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}
