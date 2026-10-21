using EInvoice.Application.Common;

namespace EInvoice.Application.Companies;

public interface ICompanyManagementService
{
    /// <summary>The company selected in the header (the Company master is filtered to it).</summary>
    Task<IReadOnlyList<CompanyListItemDto>> GetCompaniesAsync();

    /// <summary>All companies, unfiltered, e.g. for assigning companies to a user.</summary>
    Task<IReadOnlyList<CompanyOption>> GetCompanyOptionsAsync();

    Task<CompanyDetailDto?> GetCompanyByIdAsync(Guid id);

    Task<ServiceResult<Guid>> CreateCompanyAsync(SaveCompanyDto dto);

    Task<ServiceResult> UpdateCompanyAsync(SaveCompanyDto dto);

    Task<ServiceResult> DeleteCompanyAsync(Guid id);
}
