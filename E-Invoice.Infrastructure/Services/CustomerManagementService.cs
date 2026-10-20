using EInvoice.Application.Common;
using EInvoice.Application.Customers;
using EInvoice.Domain.Entities;
using EInvoice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EInvoice.Infrastructure.Services;

public class CustomerManagementService : ICustomerManagementService
{
    private readonly EInvoiceDbContext _dbContext;

    public CustomerManagementService(EInvoiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CustomerListItemDto>> GetCustomersAsync()
    {
        return await _dbContext.Customers
            .OrderBy(c => c.Name)
            .Select(c => new CustomerListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                CompanyName = _dbContext.Companies.Where(company => company.Id == c.CompanyId).Select(company => company.Name).FirstOrDefault() ?? string.Empty,
                Address1 = c.Address1,
                City = _dbContext.Cities.Where(city => city.Id == c.CityId).Select(city => city.Name).FirstOrDefault() ?? string.Empty,
                Country = _dbContext.Countries.Where(country => country.Id == c.CountryId).Select(country => country.Name).FirstOrDefault() ?? string.Empty,
                Phone = c.Phone,
                Email = c.Email,
                Trn = c.Trn,
                Tin = c.Tin,
                CreatedAtUtc = c.CreatedAtUtc,
                CreatedBy = c.CreatedBy,
                ModifiedAtUtc = c.ModifiedAtUtc,
                ModifiedBy = c.ModifiedBy,
            })
            .ToListAsync();
    }

    public async Task<CustomerDetailDto?> GetCustomerByIdAsync(Guid id)
    {
        var customer = await _dbContext.Customers.FindAsync(id);
        if (customer is null)
        {
            return null;
        }

        var cityName = await _dbContext.Cities.Where(c => c.Id == customer.CityId).Select(c => c.Name).FirstOrDefaultAsync();
        var countryName = await _dbContext.Countries.Where(c => c.Id == customer.CountryId).Select(c => c.Name).FirstOrDefaultAsync();

        return new CustomerDetailDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Address1 = customer.Address1,
            Street = customer.Street,
            CompanyId = customer.CompanyId,
            CompanyName = customer.CompanyId is null
                ? string.Empty
                : await _dbContext.Companies.Where(company => company.Id == customer.CompanyId).Select(company => company.Name).FirstOrDefaultAsync() ?? string.Empty,
            CityId = customer.CityId,
            City = cityName ?? string.Empty,
            CountryId = customer.CountryId,
            Country = countryName ?? string.Empty,
            PostBox = customer.PostBox,
            Trn = customer.Trn,
            Tin = customer.Tin,
            PeppolId = customer.PeppolId,
            Phone = customer.Phone,
            Email = customer.Email,
            Website = customer.Website,
            CreatedAtUtc = customer.CreatedAtUtc,
            CreatedBy = customer.CreatedBy,
            ModifiedAtUtc = customer.ModifiedAtUtc,
            ModifiedBy = customer.ModifiedBy,
        };
    }

    public async Task<ServiceResult<Guid>> CreateCustomerAsync(SaveCustomerDto dto)
    {
        var validationError = await ValidateLocationAsync(dto);
        if (validationError is not null)
        {
            return ServiceResult<Guid>.Failure(validationError);
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Address1 = dto.Address1,
            Street = dto.Street,
            CompanyId = dto.CompanyId,
            CityId = dto.CityId,
            CountryId = dto.CountryId,
            PostBox = dto.PostBox,
            Trn = dto.Trn,
            Tin = dto.Tin,
            PeppolId = dto.PeppolId,
            Phone = dto.Phone,
            Email = dto.Email,
            Website = dto.Website,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = dto.PerformedBy,
        };

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        return ServiceResult<Guid>.Success(customer.Id);
    }

    public async Task<ServiceResult> UpdateCustomerAsync(SaveCustomerDto dto)
    {
        if (dto.Id is null)
        {
            return ServiceResult.Failure("Customer id is required.");
        }

        var customer = await _dbContext.Customers.FindAsync(dto.Id.Value);
        if (customer is null)
        {
            return ServiceResult.Failure("Customer not found.");
        }

        var validationError = await ValidateLocationAsync(dto);
        if (validationError is not null)
        {
            return ServiceResult.Failure(validationError);
        }

        customer.Name = dto.Name;
        customer.Address1 = dto.Address1;
        customer.Street = dto.Street;
        customer.CompanyId = dto.CompanyId;
        customer.CityId = dto.CityId;
        customer.CountryId = dto.CountryId;
        customer.PostBox = dto.PostBox;
        customer.Trn = dto.Trn;
        customer.Tin = dto.Tin;
        customer.PeppolId = dto.PeppolId;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;
        customer.Website = dto.Website;
        customer.ModifiedAtUtc = DateTime.UtcNow;
        customer.ModifiedBy = dto.PerformedBy;

        await _dbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteCustomerAsync(Guid id)
    {
        var customer = await _dbContext.Customers.FindAsync(id);
        if (customer is null)
        {
            return ServiceResult.Failure("Customer not found.");
        }

        return await DeleteGuard.DeleteAsync(_dbContext, customer, id, customer.Name);
    }

    private async Task<string?> ValidateLocationAsync(SaveCustomerDto dto)
    {
        if (dto.CompanyId is null)
        {
            return "Company is required.";
        }

        if (await _dbContext.Companies.FindAsync(dto.CompanyId.Value) is null)
        {
            return "Selected company does not exist.";
        }

        if (!string.IsNullOrEmpty(dto.Trn) && dto.Trn.Length != 15)
        {
            return "TRN must be exactly 15 characters when provided.";
        }

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
