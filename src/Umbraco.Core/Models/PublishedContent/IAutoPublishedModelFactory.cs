namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Provides a live published model creation service.
/// </summary>
public interface IAutoPublishedModelFactory : IPublishedModelFactory
{
    /// <summary>
    ///     Gets an object that can be used to synchronize access to the factory.
    /// </summary>
    object SyncRoot { get; }

    /// <summary>
    ///     If the live model factory
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    ///     Tells the factory that it should build a new generation of models
    /// </summary>
    void Reset();
}
