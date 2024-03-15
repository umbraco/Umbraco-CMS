using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     A security stamp validator for the back office
/// </summary>
public class BackOfficeSecurityStampValidator : SecurityStampValidator<BackOfficeIdentityUser>
{
    public BackOfficeSecurityStampValidator(
        IOptions<BackOfficeSecurityStampValidatorOptions> options,
        BackOfficeSignInManager signInManager,
        ILoggerFactory logger)
        : base(options, signInManager, logger)
    {
    }
}
