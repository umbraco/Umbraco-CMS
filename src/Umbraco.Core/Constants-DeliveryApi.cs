namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Defines constants for the Delivery API.
    /// </summary>
    public static class DeliveryApi
    {
        /// <summary>
        ///     Constants for Delivery API routing purposes.
        /// </summary>
        public static class Routing
        {
            /// <summary>
            ///     Path prefix for unpublished content requested in a preview context.
            /// </summary>
            public const string PreviewContentPathPrefix = "preview-";
        }

        /// <summary>
        ///     Constants for Delivery API output cache.
        /// </summary>
        public static class OutputCache
        {
            /// <summary>
            ///     Output cache policy name for content
            /// </summary>
            public const string ContentCachePolicy = "DeliveryApiContent";

            /// <summary>
            ///     Output cache policy name for media
            /// </summary>
            public const string MediaCachePolicy = "DeliveryApiMedia";
        }
    }
}
