namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains constants for caching.
    /// </summary>
    public static class Cache
    {
        /// <summary>
        ///     Contains cache tag constants used for cache invalidation.
        /// </summary>
        public static class Tags
        {
            /// <summary>
            ///     The cache tag for content items.
            /// </summary>
            public const string Content = "content";

            /// <summary>
            ///     The cache tag for media items.
            /// </summary>
            public const string Media = "media";
        }

        /// <summary>
        /// Defines the string used to represent a null value in the cache.
        /// </summary>
        /// <remarks>
        /// Used in conjunction with the option to cache null values on the repository caches, so we
        /// can distinguish a true null "not found" value and a cached null value.</remarks>
        public const string NullRepresentationInCache = "*NULL*";
    }
}
