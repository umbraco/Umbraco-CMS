using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Api.Management.NotificationHandlers;

/// <summary>
/// Reports back-office authentication configuration that is unsafe, ineffective, or no longer supported.
/// </summary>
internal sealed class BackOfficeAuthConfigurationStartupHandler : INotificationHandler<UmbracoApplicationStartingNotification>
{
    private const string RemovedTokenCookieSection = "BackOfficeTokenCookie";

    private static readonly string[] RemovedCallbackPathKeys =
    [
        "AuthorizeCallbackPathName",
        "AuthorizeCallbackLogoutPathName",
        "AuthorizeCallbackErrorPathName",
    ];

    private readonly SecuritySettings _securitySettings;
    private readonly GlobalSettings _globalSettings;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackOfficeAuthConfigurationStartupHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeAuthConfigurationStartupHandler"/> class.
    /// </summary>
    public BackOfficeAuthConfigurationStartupHandler(
        IOptions<SecuritySettings> securitySettings,
        IOptions<GlobalSettings> globalSettings,
        IHostEnvironment hostEnvironment,
        IConfiguration configuration,
        ILogger<BackOfficeAuthConfigurationStartupHandler> logger)
    {
        _securitySettings = securitySettings.Value;
        _globalSettings = globalSettings.Value;
        _hostEnvironment = hostEnvironment;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Handle(UmbracoApplicationStartingNotification notification)
    {
        // An unparseable value is reported by ConfigureBackOfficeCookieOptions when the cookie is configured;
        // warning about SameSite=None here as well would be misleading, since that is not what gets applied.
        var sameSiteIsNone =
            ConfigureBackOfficeCookieOptions.TryParseAuthCookieSameSite(_securitySettings.AuthCookieSameSite, out SameSiteMode sameSite)
            && sameSite is SameSiteMode.None;

        if (sameSiteIsNone)
        {
            WarnIfPermissiveOutsideDevelopment();
            WarnIfCookieCannotBeMarkedSecure();
        }

        WarnOnRemovedSettings();
    }

    private void WarnIfPermissiveOutsideDevelopment()
    {
        if (_hostEnvironment.IsDevelopment())
        {
            return;
        }

        _logger.LogWarning(
            "The back-office authentication cookie is configured with SameSite=None in the {EnvironmentName} environment. "
            + "SameSite is what stops a cross-site request from carrying the authentication cookie, and the Management API "
            + "has no antiforgery tokens of its own, so this leaves it open to cross-site request forgery. It is intended "
            + "for development setups serving the back office from a different origin than the server. Review the "
            + "Umbraco:CMS:Security:AuthCookieSameSite configuration.",
            _hostEnvironment.EnvironmentName);
    }

    // Deliberately not gated on the environment: this combination is most likely in the very development and
    // containerised setups that SameSite=None exists for, and it breaks sign-in outright rather than weakening it.
    private void WarnIfCookieCannotBeMarkedSecure()
    {
        if (_globalSettings.UseHttps)
        {
            return;
        }

        _logger.LogWarning(
            "The back-office authentication cookie is configured with SameSite=None while Umbraco:CMS:Global:UseHttps is "
            + "false. SameSite=None requires the Secure attribute, and with UseHttps disabled the cookie is only marked "
            + "Secure when the request itself arrives over HTTPS - which it does not behind a proxy that terminates TLS. "
            + "Browsers reject a SameSite=None cookie that is not Secure, so sign-in fails with no server-side error. "
            + "Set Umbraco:CMS:Global:UseHttps to true, which marks the cookie Secure regardless of how the request reaches "
            + "the application.");
    }

    private void WarnOnRemovedSettings()
    {
        IConfigurationSection securitySection = _configuration.GetSection(Constants.Configuration.ConfigSecurity);

        List<string> configured = RemovedCallbackPathKeys
            .Where(key => securitySection.GetSection(key).Exists())
            .ToList();

        if (securitySection.GetSection(RemovedTokenCookieSection).Exists())
        {
            configured.Add(RemovedTokenCookieSection);
        }

        if (configured.Count == 0)
        {
            return;
        }

        _logger.LogWarning(
            "These Umbraco:CMS:Security configuration keys are no longer supported and are being ignored: {RemovedKeys}. "
            + "The AuthorizeCallback* paths were replaced by Security:CallbackPathName, which is the path the back office is "
            + "served at and from which the logout and error paths are derived - the meaning differs, so it needs setting "
            + "rather than renaming. BackOfficeTokenCookie was replaced by Security:AuthCookieName. Until they are migrated "
            + "the back office may redirect to a path that does not exist.",
            string.Join(", ", configured));
    }
}
