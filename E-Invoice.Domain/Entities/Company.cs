using EInvoice.Domain.Common;

namespace EInvoice.Domain.Entities;

public class Company : AuditableEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? Street { get; set; }

    public Guid CityId { get; set; }

    public Guid CountryId { get; set; }

    public string? PoBox { get; set; }

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Website { get; set; }

    /// <summary>Logo image bytes (PNG, JPEG or WebP); stored in the database because the host's disk is not persistent.</summary>
    public byte[]? LogoContent { get; set; }

    public string? LogoContentType { get; set; }

    public string? ContactPersonName { get; set; }

    public string? ContactPersonPhone { get; set; }

    /// <summary>UAE Tax Registration Number (15 digits).</summary>
    public string? Trn { get; set; }

    public string? PeppolParticipantId { get; set; }

    public string? Tin { get; set; }

    public string? BankAccountHolderName { get; set; }

    public string? BankName { get; set; }

    public string? BankAccountNumber { get; set; }

    public string? Iban { get; set; }

    public string? SwiftCode { get; set; }

    public Guid? BankCountryId { get; set; }

    /// <summary>ISO 4217 currency code, e.g. AED.</summary>
    public string? BankCurrency { get; set; }
}
