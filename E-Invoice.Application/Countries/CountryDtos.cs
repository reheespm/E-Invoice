namespace EInvoice.Application.Countries;

public class CountryListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CityCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}

public class CountryDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SaveCountryDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Identity name of the signed-in user performing the create/update, supplied by the caller.</summary>
    public string? PerformedBy { get; set; }
}
