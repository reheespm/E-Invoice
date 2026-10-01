using EInvoice.Domain.Authorization;

namespace EInvoice.Application.Dashboard;

public record DashboardWidgetInfo(string Key, string Title, string Description, string? RequiredPermission);

/// <summary>
/// Central registry of every widget a user can add to their dashboard. Add an entry here
/// to make a new widget available in the "Customize Dashboard" picker - the picker and the
/// default selection both read from <see cref="GetAll"/>.
/// </summary>
public static class DashboardWidgets
{
    public const string UsersSummary = "users-summary";
    public const string CompaniesSummary = "companies-summary";
    public const string LocationsSummary = "locations-summary";
    public const string QuickLinks = "quick-links";

    public static IReadOnlyList<DashboardWidgetInfo> GetAll() => new List<DashboardWidgetInfo>
    {
        new(UsersSummary, "Users Summary", "Total, active and locked-out users, plus role count.", Permissions.User.View),
        new(CompaniesSummary, "Companies Summary", "Total number of companies on record.", Permissions.Company.View),
        new(LocationsSummary, "Countries & Cities", "Total countries and cities configured.", Permissions.Country.View),
        new(QuickLinks, "Quick Links", "Shortcuts to common admin screens.", null),
    };

    public static IReadOnlyList<string> DefaultKeys { get; } = GetAll().Select(w => w.Key).ToList();
}
