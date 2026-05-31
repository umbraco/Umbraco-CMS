namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Specifies how the application main URL is detected from incoming HTTP requests.
/// </summary>
public enum ApplicationUrlDetection
{
    /// <summary>
    ///     No auto-detection. The application URL must be explicitly configured
    ///     via <see cref="WebRoutingSettings.UmbracoApplicationUrl" />.
    ///     Operations that require a URL (invitations, password resets) will fail
    ///     if no explicit URL is configured.
    /// </summary>
    None,

    /// <summary>
    ///     The URL is set from the first HTTP request and then locked.
    ///     Subsequent requests with different host headers are ignored.
    /// </summary>
    FirstRequest,

    /// <summary>
    ///     The URL is updated from every new incoming HTTP request (legacy behavior).
    ///     This is vulnerable to host header poisoning.
    /// </summary>
    EveryRequest,
}
