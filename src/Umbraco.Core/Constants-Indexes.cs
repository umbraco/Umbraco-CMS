namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains constants for Umbraco Search index names.
    /// </summary>
    public static class UmbracoIndexes
    {
        /// <summary>
        ///     The name of the default published content index used for template searches via <see cref="IPublishedContentQuery" />.
        /// </summary>
        public const string PublishedContentIndexName = "Umb_PublishedContent";
    }
}
