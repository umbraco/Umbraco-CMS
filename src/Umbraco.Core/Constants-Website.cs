namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Constants related to the website rendering pipeline.
    /// </summary>
    public static class Website
    {
        /// <summary>
        ///     Constants related to website output caching.
        /// </summary>
        public static class OutputCache
        {
            /// <summary>
            ///     The named output cache policy for Umbraco website content.
            /// </summary>
            public const string ContentCachePolicy = "UmbracoWebsiteContent";

            /// <summary>
            ///     Tag prefix for a specific content item (followed by the content key).
            /// </summary>
            public const string ContentTagPrefix = "umb-content-";

            /// <summary>
            ///     Tag prefix for ancestor-based eviction (followed by the ancestor content key).
            /// </summary>
            public const string AncestorTagPrefix = "umb-content-ancestor-";

            /// <summary>
            ///     Tag prefix for content type-based eviction (followed by the content type alias).
            /// </summary>
            public const string ContentTypeTagPrefix = "umb-content-type-";

            /// <summary>
            ///     Tag that matches all cached website content pages.
            /// </summary>
            public const string AllContentTag = "umb-content-all";
        }
    }
}
