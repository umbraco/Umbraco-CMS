namespace Umbraco.Cms.Core.Configuration.Models;

[UmbracoOptions(Constants.Configuration.ConfigHelpPage)]
public class HelpPageSettings
{
    /// <summary>
    ///     Gets or sets the allowed addresses to retrieve data for the content dashboard.
    /// </summary>
    public ISet<string> HelpPageUrlAllowList { get; set; } = new HashSet<string>();
}
