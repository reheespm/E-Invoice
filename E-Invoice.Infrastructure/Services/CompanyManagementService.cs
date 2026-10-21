using Microsoft.EntityFrameworkCore;
using EInvoice.Application.Common;
using EInvoice.Application.Companies;
using EInvoice.Domain.Entities;
using EInvoice.Infrastructure.Persistence;

namespace EInvoice.Infrastructure.Services;

public class CompanyManagementService : ICompanyManagementService
{
    private readonly EInvoiceDbContext _dbContext;
    private readonly ICurrentCompanyService _currentCompany;

    public CompanyManagementService(EInvoiceDbContext dbContext, ICurrentCompanyService currentCompany)
    {
        _dbContext = dbContext;
        _currentCompany = currentCompany;
    }

    /// <summary>The company selected in the header (the list is filtered to it).</summary>
    public async Task<IReadOnlyList<CompanyListItemDto>> GetCompaniesAsync()
    {
        var companyId = await _currentCompany.GetSelectedCompanyIdAsync();
        if (companyId is null)
        {
            return Array.Empty<CompanyListItemDto>();
        }

        return await _dbContext.Companies
            .Where(c => c.Id == companyId)
            .OrderBy(c => c.Name)
            .Select(c => new CompanyListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                City = _dbContext.Cities.Where(city => city.Id == c.CityId).Select(city => city.Name).FirstOrDefault() ?? string.Empty,
                Country = _dbContext.Countries.Where(co => co.Id == c.CountryId).Select(co => co.Name).FirstOrDefault() ?? string.Empty,
                Phone = c.Phone,
                Email = c.Email,
                CreatedAtUtc = c.CreatedAtUtc,
                CreatedBy = c.CreatedBy,
                ModifiedAtUtc = c.ModifiedAtUtc,
                ModifiedBy = c.ModifiedBy,
                AssignedUserCount = _dbContext.UserCompanies.Count(uc => uc.CompanyId == c.Id),
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CompanyOption>> GetCompanyOptionsAsync()
    {
        return await _dbContext.Companies
            .OrderBy(c => c.Name)
            .Select(c => new CompanyOption(c.Id, c.Name))
            .ToListAsync();
    }

    public async Task<CompanyDetailDto?> GetCompanyByIdAsync(Guid id)
    {
        var company = await FindAssignedAsync(id);
        if (company is null)
        {
            return null;
        }

        var cityName = await _dbContext.Cities.Where(c => c.Id == company.CityId).Select(c => c.Name).FirstOrDefaultAsync();
        var countryName = await _dbContext.Countries.Where(c => c.Id == company.CountryId).Select(c => c.Name).FirstOrDefaultAsync();
        var bankCountryName = company.BankCountryId is null
            ? null
            : await _dbContext.Countries.Where(c => c.Id == company.BankCountryId).Select(c => c.Name).FirstOrDefaultAsync();

        return new CompanyDetailDto
        {
            Id = company.Id,
            Name = company.Name,
            Address = company.Address,
            Street = company.Street,
            CityId = company.CityId,
            City = cityName ?? string.Empty,
            CountryId = company.CountryId,
            Country = countryName ?? string.Empty,
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
            BankCountry = bankCountryName,
            BankCurrency = company.BankCurrency,
            LogoContent = company.LogoContent,
            LogoContentType = company.LogoContentType,
            CreatedAtUtc = company.CreatedAtUtc,
            CreatedBy = company.CreatedBy,
            ModifiedAtUtc = company.ModifiedAtUtc,
            ModifiedBy = company.ModifiedBy,
        };
    }

    public async Task<ServiceResult<Guid>> CreateCompanyAsync(SaveCompanyDto dto)
    {
        var validationError = await ValidateAsync(dto);
        if (validationError is not null)
        {
            return ServiceResult<Guid>.Failure(validationError);
        }

        var company = new Company
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = dto.PerformedBy,
        };
        Apply(company, dto);

        _dbContext.Companies.Add(company);

        // The creator gets access straight away, so the new company appears in their company selector.
        var userId = (await _currentCompany.GetAsync()).UserId;
        if (userId is not null)
        {
            _dbContext.UserCompanies.Add(new UserCompany { UserId = userId.Value, CompanyId = company.Id });
        }

        await _dbContext.SaveChangesAsync();

        return ServiceResult<Guid>.Success(company.Id);
    }

    public async Task<ServiceResult> UpdateCompanyAsync(SaveCompanyDto dto)
    {
        if (dto.Id is null)
        {
            return ServiceResult.Failure("Company id is required.");
        }

        var company = await FindAssignedAsync(dto.Id.Value);
        if (company is null)
        {
            return ServiceResult.Failure("Company not found.");
        }

        var validationError = await ValidateAsync(dto);
        if (validationError is not null)
        {
            return ServiceResult.Failure(validationError);
        }

        Apply(company, dto);
        company.ModifiedAtUtc = DateTime.UtcNow;
        company.ModifiedBy = dto.PerformedBy;

        await _dbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteCompanyAsync(Guid id)
    {
        var company = await FindAssignedAsync(id);
        if (company is null)
        {
            return ServiceResult.Failure("Company not found.");
        }

        return await DeleteGuard.DeleteAsync(_dbContext, company, id, company.Name);
    }

    /// <summary>Only companies assigned to the signed-in user can be opened, changed or deleted.</summary>
    private async Task<Company?> FindAssignedAsync(Guid id)
    {
        var current = await _currentCompany.GetAsync();
        return current.Companies.Any(c => c.Id == id) ? await _dbContext.Companies.FindAsync(id) : null;
    }

    private static void Apply(Company company, SaveCompanyDto dto)
    {
        company.Name = dto.Name;
        company.Address = dto.Address;
        company.Street = dto.Street;
        company.CityId = dto.CityId;
        company.CountryId = dto.CountryId;
        company.PoBox = dto.PoBox;
        company.Phone = dto.Phone;
        company.Email = dto.Email;
        company.Website = dto.Website;
        company.ContactPersonName = Blank(dto.ContactPersonName);
        company.ContactPersonPhone = Blank(dto.ContactPersonPhone);
        company.Trn = Blank(dto.Trn);
        company.PeppolParticipantId = Blank(dto.PeppolParticipantId);
        company.Tin = Blank(dto.Tin);
        company.BankAccountHolderName = Blank(dto.BankAccountHolderName);
        company.BankName = Blank(dto.BankName);
        company.BankAccountNumber = Blank(dto.BankAccountNumber);
        company.Iban = CompanyRules.NormalizeCode(dto.Iban);
        company.SwiftCode = CompanyRules.NormalizeCode(dto.SwiftCode);
        company.BankCountryId = dto.BankCountryId;
        company.BankCurrency = Blank(dto.BankCurrency);

        if (dto.LogoContent is not null)
        {
            company.LogoContent = dto.LogoContent;
            company.LogoContentType = CompanyRules.DetectLogoContentType(dto.LogoContent);
        }
        else if (dto.RemoveLogo)
        {
            company.LogoContent = null;
            company.LogoContentType = null;
        }
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<string?> ValidateAsync(SaveCompanyDto dto)
    {
        var country = await _dbContext.Countries.FindAsync(dto.CountryId);
        if (country is null)
        {
            return "Selected country does not exist.";
        }

        var city = await _dbContext.Cities.FindAsync(dto.CityId);
        if (city is null || city.CountryId != dto.CountryId)
        {
            return "Selected city does not belong to the selected country.";
        }

        if (dto.BankCountryId is not null && await _dbContext.Countries.FindAsync(dto.BankCountryId.Value) is null)
        {
            return "Selected bank country does not exist.";
        }

        if (!string.IsNullOrWhiteSpace(dto.BankCurrency) && !Currencies.IsSupported(dto.BankCurrency))
        {
            return "Selected bank currency is not supported.";
        }

        if (!CompanyRules.IsValidTrn(dto.Trn))
        {
            return CompanyRules.TrnMessage;
        }

        if (!CompanyRules.IsValidPeppolId(dto.PeppolParticipantId))
        {
            return CompanyRules.PeppolMessage;
        }

        if (!CompanyRules.IsValidIban(dto.Iban))
        {
            return CompanyRules.IbanMessage;
        }

        if (!CompanyRules.IsValidSwift(dto.SwiftCode))
        {
            return CompanyRules.SwiftMessage;
        }

        if (dto.LogoContent is not null && CompanyRules.DetectLogoContentType(dto.LogoContent) is null)
        {
            return CompanyRules.LogoMessage;
        }

        return null;
    }
}
