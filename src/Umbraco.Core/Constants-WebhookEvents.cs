namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class WebhookEvents
    {
        public static class Aliases
        {
            /// <summary>
            ///     Webhook event alias for content emptied recycle bin.
            /// </summary>
            public const string ContentEmptiedRecycleBin = "Umbraco.ContentEmptiedRecycleBin";

            /// <summary>
            ///     Webhook event alias for content rolled back.
            /// </summary>
            public const string ContentRolledBack = "Umbraco.ContentRolledBack";

            /// <summary>
            ///     Webhook event alias for content saved.
            /// </summary>
            public const string ContentSaved = "Umbraco.ContentSaved";

            /// <summary>
            ///     Webhook event alias for content publish.
            /// </summary>
            public const string ContentPublish = "Umbraco.ContentPublish";

            /// <summary>
            ///     Webhook event alias for content delete.
            /// </summary>
            public const string ContentDelete = "Umbraco.ContentDelete";

            /// <summary>
            ///     Webhook event alias for content unpublish.
            /// </summary>
            public const string ContentUnpublish = "Umbraco.ContentUnpublish";

            /// <summary>
            ///     Webhook event alias for media delete.
            /// </summary>
            public const string MediaDelete = "Umbraco.MediaDelete";

            /// <summary>
            ///     Webhook event alias for media save.
            /// </summary>
            public const string MediaSave = "Umbraco.MediaSave";
        }

        public static class Types
        {
            /// <summary>
            ///     Webhook event type for content.
            /// </summary>
            public const string Content = "Content";

            /// <summary>
            ///     Webhook event type for content media.
            /// </summary>
            public const string Media = "Media";

            /// <summary>
            ///     Webhook event type for content member.
            /// </summary>
            public const string Member = "Member";

            /// <summary>
            ///     Webhook event type for others, this is the default category if you have not chosen one.
            /// </summary>
            public const string Other = "Other";
        }
    }
}
