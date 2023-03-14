using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Configures the back office security stamp options
/// </summary>
public class
    ConfigureBackOfficeSecurityStampValidatorOptions : IConfigureOptions<BackOfficeSecurityStampValidatorOptions>
{
    public void Configure(BackOfficeSecurityStampValidatorOptions options)
        => ConfigureSecurityStampOptions.ConfigureOptions(options);
}
