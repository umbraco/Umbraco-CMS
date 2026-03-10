using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Web.Common.Security;

/// <summary>
///     Configures the member security stamp options.
/// </summary>
public class ConfigureMemberSecurityStampValidatorOptions : IConfigureOptions<MemberSecurityStampValidatorOptions>
{
    private static readonly TimeSpan DefaultInterval = new SecurityStampValidatorOptions().ValidationInterval;
    private static readonly TimeSpan MemberValidationInterval = TimeSpan.FromSeconds(30);

    private readonly SecuritySettings _securitySettings;

    public ConfigureMemberSecurityStampValidatorOptions(IOptions<SecuritySettings> securitySettings)
        => _securitySettings = securitySettings.Value;

    /// <inheritdoc />
    public void Configure(MemberSecurityStampValidatorOptions options)
    {
        // Capture the interval before the shared configuration runs, so we can detect
        // whether a developer has already customized it vs. it being the framework default.
        TimeSpan originalInterval = options.ValidationInterval;

        // Apply the shared configuration (claim merging, etc.) which also sets
        // ValidationInterval to TimeSpan.Zero when concurrent logins are disallowed.
        ConfigureSecurityStampOptions.ConfigureOptions(options, _securitySettings.GetMemberAllowConcurrentLogins());

        // Override the validation interval for members, but only when the original value
        // was the framework default (30 minutes). If a developer has explicitly customized
        // the interval (e.g. to TimeSpan.Zero for per-request validation), respect that.
        // The backoffice needs TimeSpan.Zero because the OpenIddict /authorize endpoint can
        // silently re-authenticate within any non-zero window. Members authenticate directly
        // via cookies with no silent re-authentication mechanism, so a short interval is
        // sufficient and avoids a per-request DB lookup on every authenticated page load.
        if (_securitySettings.GetMemberAllowConcurrentLogins() is false
            && originalInterval == DefaultInterval)
        {
            options.ValidationInterval = MemberValidationInterval;
        }
    }
}
