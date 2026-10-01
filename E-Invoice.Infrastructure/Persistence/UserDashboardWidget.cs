namespace EInvoice.Infrastructure.Persistence;

public class UserDashboardWidget
{
    public Guid UserId { get; set; }

    public string WidgetKey { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
