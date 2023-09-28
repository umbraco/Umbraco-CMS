namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class WebhookEvents
    {
        /// <summary>
        ///     Webhook event name for content publish.
        /// </summary>
        public const string ContentPublish = "ContentPublish";

        /// <summary>
        ///     Webhook event name for content delete.
        /// </summary>
        public const string ContentDelete = "ContentDelete";

        /// <summary>
        ///     Webhook event name for content unpublish.
        /// </summary>
        public const string ContentUnpublish = "ContentUnpublish";

        /// <summary>
        ///     Webhook event name for media delete.
        /// </summary>
        public const string MediaDelete = "MediaDelete";

        /// <summary>
        ///     Webhook event name for media save.
        /// </summary>
        public const string MediaSave = "MediaSave";
    }
}
