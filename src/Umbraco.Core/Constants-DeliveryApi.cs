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
            ///     Output cache policy name for content.
            /// </summary>
            public const string ContentCachePolicy = "DeliveryApiContent";

            /// <summary>
            ///     Output cache policy name for media.
            /// </summary>
            public const string MediaCachePolicy = "DeliveryApiMedia";

            /// <summary>
            ///     Tag prefix for a specific content item (followed by the content key).
            /// </summary>
            public const string ContentTagPrefix = "umb-dapi-content-";

            /// <summary>
            ///     Tag prefix for a specific media item (followed by the media key).
            /// </summary>
            public const string MediaTagPrefix = "umb-dapi-media-";

            /// <summary>
            ///     Tag prefix for ancestor-based eviction (followed by the ancestor content key).
            /// </summary>
            public const string AncestorTagPrefix = "umb-dapi-content-ancestor-";

            /// <summary>
            ///     Tag prefix for content type-based eviction (followed by the content type alias).
            /// </summary>
            public const string ContentTypeTagPrefix = "umb-dapi-content-type-";

            /// <summary>
            ///     Tag that matches all cached Delivery API content responses.
            /// </summary>
            public const string AllContentTag = "umb-dapi-content-all";

            /// <summary>
            ///     Tag that matches all cached Delivery API media responses.
            /// </summary>
            public const string AllMediaTag = "umb-dapi-media-all";

            /// <summary>
            ///     Tag that matches all cached Delivery API responses (content and media).
            /// </summary>
            public const string AllTag = "umb-dapi-all";
        }

        /// <summary>
        ///     Constants for Delivery API header names.
        /// </summary>
        public static class HeaderNames
        {
            /// <summary>
            /// Header name for accept language.
            /// </summary>
            public const string AcceptLanguage = "Accept-Language";

            /// <summary>
            /// Header name for accept segment.
            /// </summary>
            public const string AcceptSegment = "Accept-Segment";

            /// <summary>
            /// Header name for API key.
            /// </summary>
            public const string ApiKey = "Api-Key";

            /// <summary>
            /// Header name for preview.
            /// </summary>
            public const string Preview = "Preview";

            /// <summary>
            /// Header name for start item.
            /// </summary>
            public const string StartItem = "Start-Item";
        }
    }
}
