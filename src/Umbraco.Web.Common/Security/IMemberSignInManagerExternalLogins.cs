using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.Security
{
    [Obsolete("This interface will be merged with IMemberSignInManager in Umbraco 10")]
    public interface IMemberSignInManagerExternalLogins : IMemberSignInManager
    {
        AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl, string? userId = null);
        Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(string? expectedXsrf = null);
        Task<IdentityResult> UpdateExternalAuthenticationTokensAsync(ExternalLoginInfo externalLogin);
        Task<SignInResult> ExternalLoginSignInAsync(ExternalLoginInfo loginInfo, bool isPersistent, bool bypassTwoFactor = false);
        Task<MemberIdentityUser> GetTwoFactorAuthenticationUserAsync();
        Task<SignInResult> TwoFactorSignInAsync(string? provider, string? code, bool isPersistent, bool rememberClient);
    }
}
