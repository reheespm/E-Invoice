using Microsoft.EntityFrameworkCore;
using EInvoice.Application.Cities;
using EInvoice.Application.Common;
using EInvoice.Domain.Entities;
using EInvoice.Infrastructure.Persistence;

namespace EInvoice.Infrastructure.Services;

public class CityManagementService : ICityManagementService
{
    private readonly EInvoiceDbContext _dbContext;

    public CityManagementService(EInvoiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CityListItemDto>> GetCitiesAsync()
    {
        return await MapListAsync(_dbContext.Cities);
    }

    public async Task<IReadOnlyList<CityListItemDto>> GetCitiesByCountryAsync(Guid countryId)
    {
        return await MapListAsync(_dbContext.Cities.Where(c => c.CountryId == countryId));
    }

    public async Task<CityDetailDto?> GetCityByIdAsync(Guid id)
    {
        var city = await _dbContext.Cities.FindAsync(id);
        if (city is null)
        {
            return null;
        }

        return new CityDetailDto
        {
            Id = city.Id,
            Name = city.Name,
            CountryId = city.CountryId,
        };
    }

    public async Task<ServiceResult<Guid>> CreateCityAsync(SaveCityDto dto)
    {
        var countryExists = await _dbContext.Countries.AnyAsync(c => c.Id == dto.CountryId);
        if (!countryExists)
        {
            return ServiceResult<Guid>.Failure("Selected country does not exist.");
        }

        var nameExists = await _dbContext.Cities.AnyAsync(c => c.Name == dto.Name && c.CountryId == dto.CountryId);
        if (nameExists)
        {
            return ServiceResult<Guid>.Failure("A city with this name already exists for the selected country.");
        }

        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            CountryId = dto.CountryId,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = dto.PerformedBy,
        };

        _dbContext.Cities.Add(city);
        await _dbContext.SaveChangesAsync();

        return ServiceResult<Guid>.Success(city.Id);
    }

    public async Task<ServiceResult> UpdateCityAsync(SaveCityDto dto)
    {
        if (dto.Id is null)
        {
            return ServiceResult.Failure("City id is required.");
        }

        var city = await _dbContext.Cities.FindAsync(dto.Id.Value);
        if (city is null)
        {
            return ServiceResult.Failure("City not found.");
        }

        var countryExists = await _dbContext.Countries.AnyAsync(c => c.Id == dto.CountryId);
        if (!countryExists)
        {
            return ServiceResult.Failure("Selected country does not exist.");
        }

        var nameExists = await _dbContext.Cities
            .AnyAsync(c => c.Name == dto.Name && c.CountryId == dto.CountryId && c.Id != dto.Id.Value);
        if (nameExists)
        {
            return ServiceResult.Failure("A city with this name already exists for the selected country.");
        }

        city.Name = dto.Name;
        city.CountryId = dto.CountryId;
        city.ModifiedAtUtc = DateTime.UtcNow;
        city.ModifiedBy = dto.PerformedBy;

        await _dbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteCityAsync(Guid id)
    {
        var city = await _dbContext.Cities.FindAsync(id);
        if (city is null)
        {
            return ServiceResult.Failure("City not found.");
        }

        return await DeleteGuard.DeleteAsync(_dbContext, city, id, city.Name);
    }

    private async Task<IReadOnlyList<CityListItemDto>> MapListAsync(IQueryable<City> query)
    {
        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CityListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                CountryId = c.CountryId,
                CountryName = _dbContext.Countries.Where(co => co.Id == c.CountryId).Select(co => co.Name).FirstOrDefault() ?? string.Empty,
                AssignedCompanyCount = _dbContext.Companies.Count(comp => comp.CityId == c.Id),
                CreatedAtUtc = c.CreatedAtUtc,
                CreatedBy = c.CreatedBy,
                ModifiedAtUtc = c.ModifiedAtUtc,
                ModifiedBy = c.ModifiedBy,
            })
            .ToListAsync();
    }
}
