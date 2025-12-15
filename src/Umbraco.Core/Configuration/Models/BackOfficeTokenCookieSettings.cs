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
}
