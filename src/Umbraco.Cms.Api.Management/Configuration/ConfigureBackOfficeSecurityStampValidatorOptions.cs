using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Api.Management.Configuration;

/// <summary>
///     Configures the back office security stamp options.
/// </summary>
public class ConfigureBackOfficeSecurityStampValidatorOptions : IConfigureOptions<BackOfficeSecurityStampValidatorOptions>
{
    private static readonly TimeSpan DefaultInterval = new SecurityStampValidatorOptions().ValidationInterval;
    private static readonly TimeSpan ConcurrentLoginsValidationInterval = TimeSpan.FromSeconds(30);

    private readonly SecuritySettings _securitySettings;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureBackOfficeSecurityStampValidatorOptions"/> class with the specified security settings and time provider.
    /// </summary>
    /// <param name="securitySettings">The <see cref="IOptions{SecuritySettings}"/> used to access security-related configuration options.</param>
    /// <param name="timeProvider">The <see cref="TimeProvider"/> used for time-based operations.</param>
    public ConfigureBackOfficeSecurityStampValidatorOptions(IOptions<SecuritySettings> securitySettings, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _securitySettings = securitySettings.Value;
    }

    /// <inheritdoc />
    public void Configure(BackOfficeSecurityStampValidatorOptions options)
    {
        options.TimeProvider = _timeProvider;

        // Capture the interval before the shared configuration runs, so a customized interval can be told apart from
        // the framework default.
        TimeSpan originalInterval = options.ValidationInterval;
        var allowConcurrentLogins = _securitySettings.GetUserAllowConcurrentLogins();
        ConfigureSecurityStampOptions.ConfigureOptions(options, allowConcurrentLogins);

        // Signing out renews the user's security stamp, which is what rejects copies of the signed out cookie. With
        // concurrent logins allowed the stamp is not checked on every request, so a copy keeps working until the next
        // check; a short interval bounds that window, unless the interval has been customized.
        if (allowConcurrentLogins && originalInterval == DefaultInterval)
        {
            options.ValidationInterval = ConcurrentLoginsValidationInterval;
        }
    }
}
