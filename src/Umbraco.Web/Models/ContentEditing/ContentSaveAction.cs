namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// The action associated with saving a content item
    /// </summary>
    public enum ContentSaveAction
    {
        /// <summary>
        /// Saves the content item, no publish
        /// </summary>
        Save = 0,

        /// <summary>
        /// Creates a new content item
        /// </summary>
        SaveNew = 1,

        /// <summary>
        /// Saves and publishes the content item
        /// </summary>
        Publish = 2,

        /// <summary>
        /// Creates and publishes a new content item
        /// </summary>
        PublishNew = 3,

        /// <summary>
        /// Saves and sends publish notification
        /// </summary>
        SendPublish = 4,

        /// <summary>
        /// Creates and sends publish notification
        /// </summary>
        SendPublishNew = 5
    }
}