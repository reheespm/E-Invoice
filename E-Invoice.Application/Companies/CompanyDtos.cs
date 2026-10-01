namespace EInvoice.Application.Companies;

public class CompanyListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
    public int AssignedUserCount { get; set; }
}

public class CompanyDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Street { get; set; }
    public Guid CityId { get; set; }
    public string City { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? PoBox { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Website { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}

public class SaveCompanyDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Street { get; set; }
    public Guid CityId { get; set; }
    public Guid CountryId { get; set; }
    public string? PoBox { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Website { get; set; }

    /// <summary>Identity name of the signed-in user performing the create/update, supplied by the caller.</summary>
    public string? PerformedBy { get; set; }
}
