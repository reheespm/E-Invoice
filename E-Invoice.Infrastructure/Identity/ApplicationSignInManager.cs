using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EInvoice.Infrastructure.Identity;

/// <summary>
/// Extends the default sign-in checks (lockout, email confirmation, ...) with the
/// application's own account-activation flag, so a deactivated user cannot sign in
/// even with a valid password.
/// </summary>
public class ApplicationSignInManager : SignInManager<ApplicationUser>
{
    public ApplicationSignInManager(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<ApplicationUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<ApplicationUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
    }

    protected override async Task<SignInResult?> PreSignInCheck(ApplicationUser user)
    {
        if (!user.IsActive)
        {
            return SignInResult.NotAllowed;
        }

        return await base.PreSignInCheck(user);
    }
}
