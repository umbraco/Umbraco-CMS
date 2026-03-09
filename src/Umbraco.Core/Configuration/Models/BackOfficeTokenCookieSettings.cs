using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for back-office token cookie settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigBackOfficeTokenCookie)]
[Obsolete("This will be replaced with a different authentication scheme. Scheduled for removal in Umbraco 18.")]
public class BackOfficeTokenCookieSettings
{
    private const string StaticSameSite = "Strict";

    /// <summary>
    ///     Gets or sets a value indicating whether the cookie SameSite configuration.
    /// </summary>
    /// <remarks>
    ///     Valid values are "Unspecified", "None", "Lax" and "Strict" (default).
    /// </remarks>
    [DefaultValue(StaticSameSite)]
    public string SameSite { get; set; } = StaticSameSite;

    /// <summary>
    ///     Gets or sets the name of the site, used to create a unique cookie name for the BackOffice token cookies.
    ///     Use this to avoid conflicts when running multiple Umbraco sites on the same domain.
    ///     The site name will be appended to the cookie name, separated by a hyphen. For example, if the site name is "siteA", the cookie names will be "__Host-umbAccessToken-siteA", "__Host-umbRefreshToken-siteA", and "umbPkceCode-siteA".
    ///     If the site name is empty, the cookie names will be "__Host-umbAccessToken", "__Host-umbRefreshToken", and "umbPkceCode".
    /// </summary>
    /// <example>"-siteA" leads to "__Host-umbAccessToken-siteA", "__Host-umbRefreshToken-siteA", and "umbPkceCode-siteA".</example>
    [DefaultValue("")]
    public string SiteName { get; set; } = string.Empty;
}
