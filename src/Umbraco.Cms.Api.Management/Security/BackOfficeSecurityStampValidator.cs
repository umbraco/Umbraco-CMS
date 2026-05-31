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
    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeSecurityStampValidator"/> class.
    /// </summary>
    /// <param name="options">The configuration options for the back office security stamp validator.</param>
    /// <param name="signInManager">The sign-in manager responsible for handling back office user sign-ins.</param>
    /// <param name="logger">The factory used to create logger instances for logging operations.</param>
    public BackOfficeSecurityStampValidator(
        IOptions<BackOfficeSecurityStampValidatorOptions> options,
        BackOfficeSignInManager signInManager,
        ILoggerFactory logger)
        : base(options, signInManager, logger)
    {
    }
}
