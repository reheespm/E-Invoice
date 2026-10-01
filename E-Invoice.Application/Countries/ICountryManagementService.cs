using EInvoice.Application.Common;

namespace EInvoice.Application.Countries;

public interface ICountryManagementService
{
    Task<IReadOnlyList<CountryListItemDto>> GetCountriesAsync();

    Task<CountryDetailDto?> GetCountryByIdAsync(Guid id);

    Task<ServiceResult<Guid>> CreateCountryAsync(SaveCountryDto dto);

    Task<ServiceResult> UpdateCountryAsync(SaveCountryDto dto);

    Task<ServiceResult> DeleteCountryAsync(Guid id);
}
