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
    ///     The URL is set from the first HTTP request and then locked against requests for other hosts.
    ///     The locked URL is only replaced when a later request is strictly more useful as a public address:
    ///     a request for a non-loopback host replaces a loopback URL, and an HTTPS request for the same host
    ///     and path replaces an HTTP URL.
    /// </summary>
    FirstRequest,

    /// <summary>
    ///     The URL is updated from every new incoming HTTP request (legacy behavior), except that it is never
    ///     replaced by a loopback address once it holds a non-loopback one, or by an HTTP address when it is
    ///     already HTTPS.
    ///     This is vulnerable to host header poisoning.
    /// </summary>
    EveryRequest,
}
