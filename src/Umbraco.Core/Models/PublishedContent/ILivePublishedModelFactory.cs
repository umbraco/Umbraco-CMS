namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a live published model creation service.
    /// </summary>
    public interface ILivePublishedModelFactory : IPublishedModelFactory
    {
        /// <summary>
        /// Gets an object that can be used to synchronize access to the factory.
        /// </summary>
        object SyncRoot { get; }

        /// <summary>
        /// Refreshes the factory.
        /// </summary>
        void Refresh();
    }
}
