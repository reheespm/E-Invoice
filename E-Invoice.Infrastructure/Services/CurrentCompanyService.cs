using System.Security.Claims;
using EInvoice.Application.Common;
using EInvoice.Infrastructure.Persistence;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EInvoice.Infrastructure.Services;

public class CurrentCompanyService : ICurrentCompanyService
{
    // Own short-lived contexts: the header's company selector and the page render at the same time in one scope,
    // so sharing the scoped DbContext would run two queries on it concurrently.
    private readonly IDbContextFactory<EInvoiceDbContext> _dbContextFactory;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private Task<CurrentCompanyInfo>? _cached;

    public CurrentCompanyService(IDbContextFactory<EInvoiceDbContext> dbContextFactory, AuthenticationStateProvider authenticationStateProvider)
    {
        _dbContextFactory = dbContextFactory;
        _authenticationStateProvider = authenticationStateProvider;
    }

    // Cached for the scope (one request, or one interactive circuit). Switching company reloads the page, which starts
    // a new scope.
    public Task<CurrentCompanyInfo> GetAsync() => _cached ??= LoadAsync();

    public async Task<Guid?> GetSelectedCompanyIdAsync() => (await GetAsync()).Selected?.Id;

    public async Task<ServiceResult> SelectAsync(Guid userId, Guid companyId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var assigned = await dbContext.UserCompanies.AnyAsync(uc => uc.UserId == userId && uc.CompanyId == companyId);
        if (!assigned)
        {
            return ServiceResult.Failure("You do not have access to the selected company.");
        }

        var user = await dbContext.Users.FindAsync(userId);
        if (user is null)
        {
            return ServiceResult.Failure("User not found.");
        }

        user.SelectedCompanyId = companyId;
        await dbContext.SaveChangesAsync();
        _cached = null;

        return ServiceResult.Success();
    }

    private async Task<CurrentCompanyInfo> LoadAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        if (!Guid.TryParse(authState.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return new CurrentCompanyInfo(null, Array.Empty<CompanyOption>(), null);
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var companies = await dbContext.UserCompanies
            .Where(uc => uc.UserId == userId)
            .Join(dbContext.Companies, uc => uc.CompanyId, c => c.Id, (uc, c) => c)
            .OrderBy(c => c.Name)
            .Select(c => new CompanyOption(c.Id, c.Name))
            .ToListAsync();

        var savedId = await dbContext.Users.Where(u => u.Id == userId).Select(u => u.SelectedCompanyId).FirstOrDefaultAsync();
        var selected = companies.FirstOrDefault(c => c.Id == savedId) ?? companies.FirstOrDefault();

        return new CurrentCompanyInfo(userId, companies, selected);
    }
}
