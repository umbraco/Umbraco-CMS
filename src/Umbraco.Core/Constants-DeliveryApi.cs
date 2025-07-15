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
