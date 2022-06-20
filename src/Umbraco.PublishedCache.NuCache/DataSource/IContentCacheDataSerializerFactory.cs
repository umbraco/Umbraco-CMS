namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

public interface IContentCacheDataSerializerFactory
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
}
