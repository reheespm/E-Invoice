using Microsoft.AspNetCore.Identity;

namespace EInvoice.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Company last chosen in the header's company selector. No foreign key on purpose: deleting a company must not be
    /// blocked by someone having it selected; an id that is no longer assigned is ignored (see CurrentCompanyService).
    /// </summary>
    public Guid? SelectedCompanyId { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
