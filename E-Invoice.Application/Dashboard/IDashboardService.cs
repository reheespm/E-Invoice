using EInvoice.Application.Common;

namespace EInvoice.Application.Dashboard;

public interface IDashboardService
{
    Task<IReadOnlyList<string>> GetSelectedWidgetKeysAsync(Guid userId);

    Task<ServiceResult> SaveSelectedWidgetKeysAsync(Guid userId, IReadOnlyList<string> widgetKeys);

    Task<IReadOnlyList<string>> GetSelectedQuickLinkKeysAsync(Guid userId);

    Task<ServiceResult> SaveSelectedQuickLinkKeysAsync(Guid userId, IReadOnlyList<string> linkKeys);
}
