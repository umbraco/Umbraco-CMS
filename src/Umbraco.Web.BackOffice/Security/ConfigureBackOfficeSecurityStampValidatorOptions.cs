using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Configures the back office security stamp options.
/// </summary>
public class ConfigureBackOfficeSecurityStampValidatorOptions : IConfigureOptions<BackOfficeSecurityStampValidatorOptions>
{
    private readonly SecuritySettings _securitySettings;

    public ConfigureBackOfficeSecurityStampValidatorOptions(IOptionsMonitor<SecuritySettings> securitySettings)
        => _securitySettings = securitySettings.CurrentValue;

    public void Configure(BackOfficeSecurityStampValidatorOptions options)
        => ConfigureSecurityStampOptions.ConfigureOptions(options, _securitySettings);
}
