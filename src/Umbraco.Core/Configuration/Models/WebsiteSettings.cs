using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for website rendering settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigWebsite)]
public class WebsiteSettings
{
    private const bool StaticEnabled = false;

    /// <summary>
    ///     Gets or sets the output cache settings for website rendering.
    /// </summary>
    public OutputCacheSettings OutputCache { get; set; } = new();

    /// <summary>
    ///     Typed configuration options for output caching of website rendering.
    /// </summary>
    public class OutputCacheSettings
    {
        private const string StaticDuration = "00:00:10"; // Ten seconds.

        /// <summary>
        ///     Gets or sets a value indicating whether website output caching is enabled.
        /// </summary>
        /// <value><c>true</c> if website output caching should be enabled; otherwise, <c>false</c>.</value>
        /// <remarks>
        ///     The default value is <c>false</c>.
        /// </remarks>
        [DefaultValue(StaticEnabled)]
        public bool Enabled { get; set; } = StaticEnabled;

        /// <summary>
        ///     Gets or sets the duration for which rendered content pages are cached.
        /// </summary>
        /// <value>Cache lifetime.</value>
        /// <remarks>
        ///     The default cache duration is ten seconds, if this configuration value is not provided.
        ///     Can be overridden per content item via <c>IWebsiteOutputCacheDurationProvider</c>.
        /// </remarks>
        [DefaultValue(StaticDuration)]
        public TimeSpan ContentDuration { get; set; } = TimeSpan.Parse(StaticDuration);
    }
}
