namespace Umbraco.Cms.Core.Models.PublishedContent
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
        /// Tells the factory that it should build a new generation of models
        /// </summary>
        void Reset();

        /// <summary>
        /// If the live model factory
        /// </summary>
        bool Enabled { get; }
    }
}
