namespace EInvoice.Infrastructure.Persistence;

public class UserQuickLink
{
    public Guid UserId { get; set; }

    public string LinkKey { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
