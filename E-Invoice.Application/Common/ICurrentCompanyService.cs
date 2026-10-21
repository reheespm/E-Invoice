namespace EInvoice.Application.Common;

public record CompanyOption(Guid Id, string Name);

/// <summary>The signed-in user's companies and the one currently selected in the header.</summary>
public record CurrentCompanyInfo(Guid? UserId, IReadOnlyList<CompanyOption> Companies, CompanyOption? Selected);

/// <summary>
/// Resolves the company the signed-in user is working in. Company-owned data (customers, invoices, ...) is filtered by it.
/// </summary>
public interface ICurrentCompanyService
{
    /// <summary>Companies assigned to the signed-in user and the selected one (the saved choice, else the first assigned).</summary>
    Task<CurrentCompanyInfo> GetAsync();

    /// <summary>The selected company id, or null when the user has no company assigned.</summary>
    Task<Guid?> GetSelectedCompanyIdAsync();

    /// <summary>Saves the user's choice; fails when the company is not assigned to the user.</summary>
    Task<ServiceResult> SelectAsync(Guid userId, Guid companyId);
}
