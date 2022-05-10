using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     A <see cref="SignInManager{BackOfficeIdentityUser}" /> for the back office with a
///     <seealso cref="BackOfficeIdentityUser" />
/// </summary>
public interface IBackOfficeSignInManager
{
    AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string? redirectUrl, string? userId = null);

    Task<SignInResult> ExternalLoginSignInAsync(ExternalLoginInfo loginInfo, bool isPersistent, bool bypassTwoFactor = false);

    Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync();

    Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(string? expectedXsrf = null);

    Task<BackOfficeIdentityUser> GetTwoFactorAuthenticationUserAsync();

    Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure);

    Task SignOutAsync();

    Task SignInAsync(BackOfficeIdentityUser user, bool isPersistent, string? authenticationMethod = null);

    Task<ClaimsPrincipal> CreateUserPrincipalAsync(BackOfficeIdentityUser user);

    Task<SignInResult> TwoFactorSignInAsync(string? provider, string? code, bool isPersistent, bool rememberClient);

    Task<IdentityResult> UpdateExternalAuthenticationTokensAsync(ExternalLoginInfo externalLogin);
}
