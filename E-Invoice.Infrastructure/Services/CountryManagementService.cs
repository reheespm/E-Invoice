using Microsoft.EntityFrameworkCore;
using EInvoice.Application.Common;
using EInvoice.Application.Countries;
using EInvoice.Domain.Entities;
using EInvoice.Infrastructure.Persistence;

namespace EInvoice.Infrastructure.Services;

public class CountryManagementService : ICountryManagementService
{
    private readonly EInvoiceDbContext _dbContext;

    public CountryManagementService(EInvoiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CountryListItemDto>> GetCountriesAsync()
    {
        return await _dbContext.Countries
            .OrderBy(c => c.Name)
            .Select(c => new CountryListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                CityCount = _dbContext.Cities.Count(city => city.CountryId == c.Id),
                CreatedAtUtc = c.CreatedAtUtc,
                CreatedBy = c.CreatedBy,
                ModifiedAtUtc = c.ModifiedAtUtc,
                ModifiedBy = c.ModifiedBy,
            })
            .ToListAsync();
    }

    public async Task<CountryDetailDto?> GetCountryByIdAsync(Guid id)
    {
        var country = await _dbContext.Countries.FindAsync(id);
        if (country is null)
        {
            return null;
        }

        return new CountryDetailDto
        {
            Id = country.Id,
            Name = country.Name,
        };
    }

    public async Task<ServiceResult<Guid>> CreateCountryAsync(SaveCountryDto dto)
    {
        var nameExists = await _dbContext.Countries.AnyAsync(c => c.Name == dto.Name);
        if (nameExists)
        {
            return ServiceResult<Guid>.Failure("A country with this name already exists.");
        }

        var country = new Country
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = dto.PerformedBy,
        };

        _dbContext.Countries.Add(country);
        await _dbContext.SaveChangesAsync();

        return ServiceResult<Guid>.Success(country.Id);
    }

    public async Task<ServiceResult> UpdateCountryAsync(SaveCountryDto dto)
    {
        if (dto.Id is null)
        {
            return ServiceResult.Failure("Country id is required.");
        }

        var country = await _dbContext.Countries.FindAsync(dto.Id.Value);
        if (country is null)
        {
            return ServiceResult.Failure("Country not found.");
        }

        var nameExists = await _dbContext.Countries.AnyAsync(c => c.Name == dto.Name && c.Id != dto.Id.Value);
        if (nameExists)
        {
            return ServiceResult.Failure("A country with this name already exists.");
        }

        country.Name = dto.Name;
        country.ModifiedAtUtc = DateTime.UtcNow;
        country.ModifiedBy = dto.PerformedBy;

        await _dbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteCountryAsync(Guid id)
    {
        var country = await _dbContext.Countries.FindAsync(id);
        if (country is null)
        {
            return ServiceResult.Failure("Country not found.");
        }

        var cityCount = await _dbContext.Cities.CountAsync(c => c.CountryId == id);
        if (cityCount > 0)
        {
            return ServiceResult.Failure($"Cannot delete '{country.Name}': {cityCount} city record(s) still reference it.");
        }

        _dbContext.Countries.Remove(country);
        await _dbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }
}
