using EInvoice.Domain.Common;

namespace EInvoice.Domain.Entities;

public class Customer : AuditableEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address1 { get; set; } = string.Empty;

    public string? Street { get; set; }

    public Guid? CompanyId { get; set; }

    public Guid CityId { get; set; }

    public Guid CountryId { get; set; }

    public string? PostBox { get; set; }

    public string? Trn { get; set; }

    public string? Tin { get; set; }

    public string? PeppolId { get; set; }

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Website { get; set; }
}
