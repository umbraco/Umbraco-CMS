namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a live published model creation service.
    /// </summary>
    public interface ILivePublishedModelFactory2 : ILivePublishedModelFactory
    {
        /// <summary>
        /// Tells the factory that it should build a new generation of models
        /// </summary>
        void Reset();

        /// <summary>
        /// If the live model factory 
        /// </summary>
        bool Enabled { get; }
    }

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
        /// <remarks>
        /// <para>This will typically re-compiled models/classes into a new DLL that are used to populate the cache.</para>
        /// <para>This is called prior to refreshing the cache.</para>
        /// </remarks>
        void Refresh();
    }
}
