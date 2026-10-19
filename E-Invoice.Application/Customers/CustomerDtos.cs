namespace EInvoice.Application.Customers;

public class CustomerListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Address1 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Trn { get; set; }
    public string? Tin { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}

public class CustomerDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address1 { get; set; } = string.Empty;
    public string? Street { get; set; }
    public Guid? CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Guid CityId { get; set; }
    public string City { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? PostBox { get; set; }
    public string? Trn { get; set; }
    public string? Tin { get; set; }
    public string? PeppolId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Website { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}

public class SaveCustomerDto
{
    public Guid? Id { get; set; }
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
    public string? PerformedBy { get; set; }
}
