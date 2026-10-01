namespace EInvoice.Application.Cities;

public class CityListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public int AssignedCompanyCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}

public class CityDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
}

public class SaveCityDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CountryId { get; set; }

    /// <summary>Identity name of the signed-in user performing the create/update, supplied by the caller.</summary>
    public string? PerformedBy { get; set; }
}
