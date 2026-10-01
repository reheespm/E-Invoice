using EInvoice.Domain.Common;

namespace EInvoice.Domain.Entities;

public class City : AuditableEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid CountryId { get; set; }
}
