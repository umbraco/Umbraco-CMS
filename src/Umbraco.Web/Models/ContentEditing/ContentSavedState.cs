namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// The saved state of a content item
    /// </summary>
    public enum ContentSavedState
    {
        /// <summary>
        /// The item isn't created yet
        /// </summary>
        NotCreated,

        /// <summary>
        /// The item is saved but isn't published
        /// </summary>
        Draft,

        /// <summary>
        /// The item is published and there are no pending changes
        /// </summary>
        Published,

        /// <summary>
        /// The item is published and there are pending changes
        /// </summary>
        PublishedPendingChanges
    }
}
