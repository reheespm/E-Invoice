using Microsoft.EntityFrameworkCore;
using EInvoice.Application.Common;
using EInvoice.Application.Dashboard;
using EInvoice.Infrastructure.Persistence;

namespace EInvoice.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private const string EmptySelectionMarker = "__none__";

    private readonly EInvoiceDbContext _dbContext;

    public DashboardService(EInvoiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<string>> GetSelectedWidgetKeysAsync(Guid userId)
    {
        var saved = await _dbContext.UserDashboardWidgets
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.SortOrder)
            .Select(w => w.WidgetKey)
            .ToListAsync();

        if (saved.Count == 0)
        {
            return DashboardWidgets.DefaultKeys;
        }

        return saved.Contains(EmptySelectionMarker) ? Array.Empty<string>() : saved;
    }

    public async Task<ServiceResult> SaveSelectedWidgetKeysAsync(Guid userId, IReadOnlyList<string> widgetKeys)
    {
        var validKeys = new HashSet<string>(DashboardWidgets.GetAll().Select(w => w.Key), StringComparer.Ordinal);
        var keysToSave = widgetKeys.Where(validKeys.Contains).Distinct().ToList();

        if (keysToSave.Count == 0)
        {
            keysToSave.Add(EmptySelectionMarker);
        }

        var existing = await _dbContext.UserDashboardWidgets.Where(w => w.UserId == userId).ToListAsync();
        _dbContext.UserDashboardWidgets.RemoveRange(existing);

        for (var i = 0; i < keysToSave.Count; i++)
        {
            _dbContext.UserDashboardWidgets.Add(new UserDashboardWidget
            {
                UserId = userId,
                WidgetKey = keysToSave[i],
                SortOrder = i,
            });
        }

        await _dbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<IReadOnlyList<string>> GetSelectedQuickLinkKeysAsync(Guid userId)
    {
        var saved = await _dbContext.UserQuickLinks
            .Where(l => l.UserId == userId)
            .OrderBy(l => l.SortOrder)
            .Select(l => l.LinkKey)
            .ToListAsync();

        if (saved.Count == 0)
        {
            return QuickLinks.DefaultKeys;
        }

        return saved.Contains(EmptySelectionMarker) ? Array.Empty<string>() : saved;
    }

    public async Task<ServiceResult> SaveSelectedQuickLinkKeysAsync(Guid userId, IReadOnlyList<string> linkKeys)
    {
        var validKeys = new HashSet<string>(QuickLinks.GetAll().Select(l => l.Key), StringComparer.Ordinal);
        var keysToSave = linkKeys.Where(validKeys.Contains).Distinct().ToList();

        if (keysToSave.Count == 0)
        {
            keysToSave.Add(EmptySelectionMarker);
        }

        var existing = await _dbContext.UserQuickLinks.Where(l => l.UserId == userId).ToListAsync();
        _dbContext.UserQuickLinks.RemoveRange(existing);

        for (var i = 0; i < keysToSave.Count; i++)
        {
            _dbContext.UserQuickLinks.Add(new UserQuickLink
            {
                UserId = userId,
                LinkKey = keysToSave[i],
                SortOrder = i,
            });
        }

        await _dbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }
}
