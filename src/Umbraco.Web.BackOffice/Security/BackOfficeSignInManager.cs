using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.BackOffice;

namespace Umbraco.Web.BackOffice.Security
{
    public class BackOfficeSignInManager : SignInManager<BackOfficeIdentityUser>
    {
        public BackOfficeSignInManager(
            UserManager<BackOfficeIdentityUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<BackOfficeIdentityUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<BackOfficeIdentityUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<BackOfficeIdentityUser> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }
    }
}
