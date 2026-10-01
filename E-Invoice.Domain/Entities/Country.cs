using EInvoice.Domain.Common;

namespace EInvoice.Domain.Entities;

public class Country : AuditableEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
