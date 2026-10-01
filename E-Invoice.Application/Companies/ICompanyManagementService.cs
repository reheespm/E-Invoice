using EInvoice.Application.Common;

namespace EInvoice.Application.Companies;

public interface ICompanyManagementService
{
    Task<IReadOnlyList<CompanyListItemDto>> GetCompaniesAsync();

    Task<CompanyDetailDto?> GetCompanyByIdAsync(Guid id);

    Task<ServiceResult<Guid>> CreateCompanyAsync(SaveCompanyDto dto);

    Task<ServiceResult> UpdateCompanyAsync(SaveCompanyDto dto);

    Task<ServiceResult> DeleteCompanyAsync(Guid id);
}
