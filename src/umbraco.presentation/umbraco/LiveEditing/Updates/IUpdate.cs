namespace umbraco.presentation.LiveEditing.Updates
{
    /// <summary>
    /// Interface for a class that holds information about a Live Editing update.
    /// </summary>
    public interface IUpdate
    {
        /// <summary>
        /// Saves the update.
        /// </summary>
        void Save();

        /// <summary>
        /// Publishes the update.
        /// </summary>
        void Publish();
    }
}
