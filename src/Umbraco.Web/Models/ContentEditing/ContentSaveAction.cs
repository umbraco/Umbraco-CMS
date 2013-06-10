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
        Save,

        /// <summary>
        /// Saves and publishes the content item
        /// </summary>
        Publish,

        /// <summary>
        /// Saves a new content item
        /// </summary>
        SaveNew,

        /// <summary>
        /// Saves an publishes a new content item
        /// </summary>
        PublishNew
    }
}