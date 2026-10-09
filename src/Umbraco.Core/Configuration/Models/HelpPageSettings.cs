namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for help page settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigHelpPage)]
[Obsolete("No longer used, as help page data is no longer retrieved from a remote source. Scheduled for removal in Umbraco 19.")]
public class HelpPageSettings
{
    /// <summary>
    ///     Gets or sets the allowed addresses to retrieve data for the content dashboard.
    /// </summary>
    public ISet<string> HelpPageUrlAllowList { get; set; } = new HashSet<string>();
}
