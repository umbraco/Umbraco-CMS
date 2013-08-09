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
        /// Saves a new content item
        /// </summary>
        SaveNew = 1,

        /// <summary>
        /// Saves and publishes the content item
        /// </summary>
        Publish = 2,

        /// <summary>
        /// Saves an publishes a new content item
        /// </summary>
        PublishNew = 3
    }
}