using System.ComponentModel;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.Configuration;

/// <summary>
///     Typed configuration options for content dashboard settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigContentDashboard)]
public class ContentDashboardSettings
{
    private const string DefaultContentDashboardPath = "cms";

    /// <summary>
    ///     Gets a value indicating whether the content dashboard should be available to all users.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the dashboard is visible for all user groups; otherwise, <c>false</c>
    ///     and the default access rules for that dashboard will be in use.
    /// </value>
    public bool AllowContentDashboardAccessToAllUsers { get; set; } = true;

    /// <summary>
    ///     Gets the path to use when constructing the URL for retrieving data for the content dashboard.
    /// </summary>
    /// <value>The URL path.</value>
    [DefaultValue(DefaultContentDashboardPath)]
    public string ContentDashboardPath { get; set; } = DefaultContentDashboardPath;

    /// <summary>
    ///     Gets the allowed addresses to retrieve data for the content dashboard.
    /// </summary>
    /// <value>The URLs.</value>
    public string[]? ContentDashboardUrlAllowlist { get; set; }
}
