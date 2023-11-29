using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.Security;

/// <summary>
///     A security stamp validator for the back office
/// </summary>
public class MemberSecurityStampValidator : SecurityStampValidator<MemberIdentityUser>
{
    public MemberSecurityStampValidator(
        IOptions<MemberSecurityStampValidatorOptions> options,
        MemberSignInManager signInManager, ISystemClock clock, ILoggerFactory logger)
        : base(options, signInManager, clock, logger)
    {
    }

    public override Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        return base.ValidateAsync(context);
    }
}
