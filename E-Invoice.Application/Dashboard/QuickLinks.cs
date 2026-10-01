using EInvoice.Domain.Authorization;

namespace EInvoice.Application.Dashboard;

public record QuickLinkInfo(string Key, string Title, string Href, string? RequiredPermission);

/// <summary>
/// Central registry of every link the Quick Links dashboard widget can show. Add an entry
/// here to make a new shortcut available in the "Customize Dashboard" picker.
/// </summary>
public static class QuickLinks
{
    public const string Users = "users";
    public const string Roles = "roles";
    public const string Companies = "companies";
    public const string Countries = "countries";
    public const string Cities = "cities";

    public static IReadOnlyList<QuickLinkInfo> GetAll() => new List<QuickLinkInfo>
    {
        new(Users, "Manage Users", "admin/users", Permissions.User.View),
        new(Roles, "Manage Roles & Permissions", "admin/roles", Permissions.Role.View),
        new(Companies, "Manage Companies", "admin/companies", Permissions.Company.View),
        new(Countries, "Manage Countries", "admin/countries", Permissions.Country.View),
        new(Cities, "Manage Cities", "admin/cities", Permissions.City.View),
    };

    public static IReadOnlyList<string> DefaultKeys { get; } = GetAll().Select(l => l.Key).ToList();
}
