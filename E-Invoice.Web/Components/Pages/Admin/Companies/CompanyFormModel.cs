using System.ComponentModel.DataAnnotations;
using EInvoice.Application.Companies;

namespace EInvoice.Web.Components.Pages.Admin.Companies;

/// <summary>Form model shared by the Add and Edit company pages.</summary>
public class CompanyFormModel
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Street { get; set; }

    [Required(ErrorMessage = "City is required.")]
    public Guid? CityId { get; set; }

    [Required(ErrorMessage = "Country is required.")]
    public Guid? CountryId { get; set; }

    [StringLength(50)]
    public string? PoBox { get; set; }

    [Required, StringLength(50)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [StringLength(256)]
    public string? Website { get; set; }

    [StringLength(200)]
    public string? ContactPersonName { get; set; }

    [StringLength(50)]
    public string? ContactPersonPhone { get; set; }

    [Trn]
    public string? Trn { get; set; }

    [PeppolId, StringLength(200)]
    public string? PeppolParticipantId { get; set; }

    [StringLength(50)]
    public string? Tin { get; set; }

    [StringLength(200)]
    public string? BankAccountHolderName { get; set; }

    [StringLength(200)]
    public string? BankName { get; set; }

    [StringLength(50)]
    public string? BankAccountNumber { get; set; }

    [Iban]
    public string? Iban { get; set; }

    [SwiftCode]
    public string? SwiftCode { get; set; }

    public Guid? BankCountryId { get; set; }

    public string? BankCurrency { get; set; }

    /// <summary>Newly uploaded logo (null = keep the current one).</summary>
    public byte[]? NewLogo { get; set; }

    public bool RemoveLogo { get; set; }

    /// <summary>data: URL of the logo currently shown in the form.</summary>
    public string? LogoPreview { get; set; }

    public static CompanyFormModel FromDetail(CompanyDetailDto company) => new()
    {
        Name = company.Name,
        Address = company.Address,
        Street = company.Street,
        CityId = company.CityId,
        CountryId = company.CountryId,
        PoBox = company.PoBox,
        Phone = company.Phone,
        Email = company.Email,
        Website = company.Website,
        ContactPersonName = company.ContactPersonName,
        ContactPersonPhone = company.ContactPersonPhone,
        Trn = company.Trn,
        PeppolParticipantId = company.PeppolParticipantId,
        Tin = company.Tin,
        BankAccountHolderName = company.BankAccountHolderName,
        BankName = company.BankName,
        BankAccountNumber = company.BankAccountNumber,
        Iban = company.Iban,
        SwiftCode = company.SwiftCode,
        BankCountryId = company.BankCountryId,
        BankCurrency = company.BankCurrency,
        LogoPreview = company.LogoContent is null
            ? null
            : $"data:{company.LogoContentType};base64,{Convert.ToBase64String(company.LogoContent)}",
    };

    public SaveCompanyDto ToDto(Guid? id, string? performedBy) => new()
    {
        Id = id,
        Name = Name,
        Address = Address,
        Street = Street,
        CityId = CityId!.Value,
        CountryId = CountryId!.Value,
        PoBox = PoBox,
        Phone = Phone,
        Email = Email,
        Website = Website,
        ContactPersonName = ContactPersonName,
        ContactPersonPhone = ContactPersonPhone,
        Trn = Trn,
        PeppolParticipantId = PeppolParticipantId,
        Tin = Tin,
        BankAccountHolderName = BankAccountHolderName,
        BankName = BankName,
        BankAccountNumber = BankAccountNumber,
        Iban = Iban,
        SwiftCode = SwiftCode,
        BankCountryId = BankCountryId,
        BankCurrency = BankCurrency,
        LogoContent = NewLogo,
        RemoveLogo = RemoveLogo,
        PerformedBy = performedBy,
    };
}
