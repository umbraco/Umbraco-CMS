using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Web.Common.Security;

/// <summary>
///     Configures the member security stamp options.
/// </summary>
public class ConfigureMemberSecurityStampValidatorOptions : IConfigureOptions<MemberSecurityStampValidatorOptions>
{
    private static readonly TimeSpan MemberValidationInterval = TimeSpan.FromSeconds(30);

    private readonly SecuritySettings _securitySettings;

    public ConfigureMemberSecurityStampValidatorOptions(IOptions<SecuritySettings> securitySettings)
        => _securitySettings = securitySettings.Value;

    /// <inheritdoc />
    public void Configure(MemberSecurityStampValidatorOptions options)
    {
        // Apply the shared configuration (claim merging, etc.) which also sets
        // ValidationInterval to TimeSpan.Zero when AllowConcurrentLogins is false.
        ConfigureSecurityStampOptions.ConfigureOptions(options, _securitySettings);

        // Override the validation interval for members. The backoffice needs TimeSpan.Zero
        // because the OpenIddict /authorize endpoint can silently re-authenticate within
        // any non-zero window. Members authenticate directly via cookies with no silent
        // re-authentication mechanism, so a short interval is sufficient and avoids a
        // per-request DB lookup on every authenticated page load.
        if (_securitySettings.AllowConcurrentLogins is false
            && options.ValidationInterval == TimeSpan.Zero)
        {
            options.ValidationInterval = MemberValidationInterval;
        }
    }
}
