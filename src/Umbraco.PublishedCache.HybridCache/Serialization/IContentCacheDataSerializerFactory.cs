namespace Umbraco.Cms.Infrastructure.HybridCache.Serialization;

internal interface IContentCacheDataSerializerFactory
{
    /// <summary>
    ///     Gets or creates a new instance of <see cref="IContentCacheDataSerializer" />
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     This method may return the same instance, however this depends on the state of the application and if any
    ///     underlying data has changed.
    ///     This method may also be used to initialize anything before a serialization/deserialization session occurs.
    /// </remarks>
    IContentCacheDataSerializer Create(ContentCacheDataSerializerEntityType types);

    /// <summary>
    ///     Discards anything the factory has derived from content type and media type definitions, so that the next
    ///     <see cref="Create" /> reflects the current definitions.
    /// </summary>
    /// <remarks>
    ///     Must be called whenever a content type or media type is changed or deleted, before anything that relies
    ///     on <see cref="Create" /> reflecting that change.
    /// </remarks>
    void ClearTypeCache();
}
