using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Configuration;

/// <summary>
///     Used to configure <see cref="CookieAuthenticationOptions" /> for the back office "exposed" authentication type
/// </summary>
public class ConfigureBackOfficeExposedCookieOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
{
    private readonly SecuritySettings _securitySettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureBackOfficeExposedCookieOptions" /> class.
    /// </summary>
    /// <param name="securitySettings">The <see cref="SecuritySettings" /> options</param>
    public ConfigureBackOfficeExposedCookieOptions(IOptions<SecuritySettings> securitySettings)
        => _securitySettings = securitySettings.Value;

    /// <inheritdoc />
    public void Configure(string? name, CookieAuthenticationOptions options)
    {
        if (name != Constants.Security.BackOfficeExposedAuthenticationType)
        {
            return;
        }

        Configure(options);
    }

    /// <inheritdoc />
    public void Configure(CookieAuthenticationOptions options)
    {
        options.Cookie.Name = _securitySettings.AuthCookieName.IsNullOrWhiteSpace()
            ? Constants.Security.BackOfficeExposedCookieName
            : $"{_securitySettings.AuthCookieName}{Constants.Security.BackOfficeExposedCookieNamePostfix}";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
    }
}
