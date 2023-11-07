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
        ///     Webhook event name for media deleted.
        /// </summary>
        public const string MediaDeleted = "Media Deleted";

        /// <summary>
        ///     Webhook event name for media saved.
        /// </summary>
        public const string MediaSaved = "Media Saved";
    }
}
