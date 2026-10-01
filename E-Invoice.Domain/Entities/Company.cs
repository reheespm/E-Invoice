using EInvoice.Domain.Common;

namespace EInvoice.Domain.Entities;

public class Company : AuditableEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CompanyCode { get; set; } = string.Empty;

    public string? PlantCode { get; set; }

    public string Address { get; set; } = string.Empty;

    public string? Street { get; set; }

    public Guid CityId { get; set; }

    public Guid CountryId { get; set; }

    public string? PoBox { get; set; }

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Website { get; set; }
}
