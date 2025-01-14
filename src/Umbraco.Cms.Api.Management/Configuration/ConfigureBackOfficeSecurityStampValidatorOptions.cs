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
    private readonly SecuritySettings _securitySettings;
    private readonly TimeProvider _timeProvider;

    public ConfigureBackOfficeSecurityStampValidatorOptions(IOptions<SecuritySettings> securitySettings, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _securitySettings = securitySettings.Value;
    }

    /// <inheritdoc />
    public void Configure(BackOfficeSecurityStampValidatorOptions options)
    {
        options.TimeProvider = _timeProvider;
        ConfigureSecurityStampOptions.ConfigureOptions(options, _securitySettings);
    }
}
