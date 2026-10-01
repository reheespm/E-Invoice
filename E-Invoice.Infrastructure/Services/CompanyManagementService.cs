using Microsoft.EntityFrameworkCore;
using EInvoice.Application.Common;
using EInvoice.Application.Companies;
using EInvoice.Domain.Entities;
using EInvoice.Infrastructure.Persistence;

namespace EInvoice.Infrastructure.Services;

public class CompanyManagementService : ICompanyManagementService
{
    private readonly EInvoiceDbContext _dbContext;

    public CompanyManagementService(EInvoiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CompanyListItemDto>> GetCompaniesAsync()
    {
        return await _dbContext.Companies
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

    public async Task<CompanyDetailDto?> GetCompanyByIdAsync(Guid id)
    {
        var company = await _dbContext.Companies.FindAsync(id);
        if (company is null)
        {
            return null;
        }

        var cityName = await _dbContext.Cities.Where(c => c.Id == company.CityId).Select(c => c.Name).FirstOrDefaultAsync();
        var countryName = await _dbContext.Countries.Where(c => c.Id == company.CountryId).Select(c => c.Name).FirstOrDefaultAsync();

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
            CreatedAtUtc = company.CreatedAtUtc,
            CreatedBy = company.CreatedBy,
            ModifiedAtUtc = company.ModifiedAtUtc,
            ModifiedBy = company.ModifiedBy,
        };
    }

    public async Task<ServiceResult<Guid>> CreateCompanyAsync(SaveCompanyDto dto)
    {
        var validationError = await ValidateLocationAsync(dto);
        if (validationError is not null)
        {
            return ServiceResult<Guid>.Failure(validationError);
        }

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Address = dto.Address,
            Street = dto.Street,
            CityId = dto.CityId,
            CountryId = dto.CountryId,
            PoBox = dto.PoBox,
            Phone = dto.Phone,
            Email = dto.Email,
            Website = dto.Website,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = dto.PerformedBy,
        };

        _dbContext.Companies.Add(company);
        await _dbContext.SaveChangesAsync();

        return ServiceResult<Guid>.Success(company.Id);
    }

    public async Task<ServiceResult> UpdateCompanyAsync(SaveCompanyDto dto)
    {
        if (dto.Id is null)
        {
            return ServiceResult.Failure("Company id is required.");
        }

        var company = await _dbContext.Companies.FindAsync(dto.Id.Value);
        if (company is null)
        {
            return ServiceResult.Failure("Company not found.");
        }

        var validationError = await ValidateLocationAsync(dto);
        if (validationError is not null)
        {
            return ServiceResult.Failure(validationError);
        }

        company.Name = dto.Name;
        company.Address = dto.Address;
        company.Street = dto.Street;
        company.CityId = dto.CityId;
        company.CountryId = dto.CountryId;
        company.PoBox = dto.PoBox;
        company.Phone = dto.Phone;
        company.Email = dto.Email;
        company.Website = dto.Website;
        company.ModifiedAtUtc = DateTime.UtcNow;
        company.ModifiedBy = dto.PerformedBy;

        await _dbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteCompanyAsync(Guid id)
    {
        var company = await _dbContext.Companies.FindAsync(id);
        if (company is null)
        {
            return ServiceResult.Failure("Company not found.");
        }

        _dbContext.Companies.Remove(company);
        await _dbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    private async Task<string?> ValidateLocationAsync(SaveCompanyDto dto)
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

        return null;
    }
}
