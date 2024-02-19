using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Configures the back office security stamp options.
/// </summary>
public class ConfigureBackOfficeSecurityStampValidatorOptions : IConfigureOptions<BackOfficeSecurityStampValidatorOptions>
{
    private readonly SecuritySettings _securitySettings;

    public ConfigureBackOfficeSecurityStampValidatorOptions()
        : this(StaticServiceProvider.Instance.GetRequiredService<IOptions<SecuritySettings>>())
    {
    }

    public ConfigureBackOfficeSecurityStampValidatorOptions(IOptions<SecuritySettings> securitySettings)
        => _securitySettings = securitySettings.Value;

    /// <inheritdoc />
    public void Configure(BackOfficeSecurityStampValidatorOptions options)
        => ConfigureSecurityStampOptions.ConfigureOptions(options, _securitySettings);
}
