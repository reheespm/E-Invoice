using EInvoice.Application.Common;

namespace EInvoice.Application.Cities;

public interface ICityManagementService
{
    Task<IReadOnlyList<CityListItemDto>> GetCitiesAsync();

    Task<IReadOnlyList<CityListItemDto>> GetCitiesByCountryAsync(Guid countryId);

    Task<CityDetailDto?> GetCityByIdAsync(Guid id);

    Task<ServiceResult<Guid>> CreateCityAsync(SaveCityDto dto);

    Task<ServiceResult> UpdateCityAsync(SaveCityDto dto);

    Task<ServiceResult> DeleteCityAsync(Guid id);
}
