using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
///     Creates and manages <see cref="IPublishedSnapshot" /> instances.
/// </summary>
public interface IPublishedSnapshotService : IDisposable
{
    /* Various places (such as Node) want to access the XML content, today as an XmlDocument
     * but to migrate to a new cache, they're migrating to an XPathNavigator. Still, they need
     * to find out how to get that navigator.
     *
     * Because a cache such as NuCache is contextual i.e. it has a "snapshot" thing and remains
     * consistent over the snapshot, the navigator has to come from the "current" snapshot.
     *
     * So although everything should be injected... we also need a notion of "the current published
     * snapshot". This is provided by the IPublishedSnapshotAccessor.
     *
     */

    /// <summary>
    ///     Creates a published snapshot.
    /// </summary>
    /// <param name="previewToken">A preview token, or <c>null</c> if not previewing.</param>
    /// <returns>A published snapshot.</returns>
    /// <remarks>
    ///     If <paramref name="previewToken" /> is null, the snapshot is not previewing, else it
    ///     is previewing, and what is or is not visible in preview depends on the content of the token,
    ///     which is not specified and depends on the actual published snapshot service implementation.
    /// </remarks>
    IPublishedSnapshot CreatePublishedSnapshot(string? previewToken);

    /// <summary>
    ///     Rebuilds internal database caches (but does not reload).
    /// </summary>
    /// <param name="contentTypeIds">
    ///     If not null will process content for the matching content types, if empty will process all
    ///     content
    /// </param>
    /// <param name="mediaTypeIds">
    ///     If not null will process content for the matching media types, if empty will process all
    ///     media
    /// </param>
    /// <param name="memberTypeIds">
    ///     If not null will process content for the matching members types, if empty will process all
    ///     members
    /// </param>
    /// <remarks>
    ///     <para>
    ///         Forces the snapshot service to rebuild its internal database caches. For instance, some caches
    ///         may rely on a database table to store pre-serialized version of documents.
    ///     </para>
    ///     <para>
    ///         This does *not* reload the caches. Caches need to be reloaded, for instance via
    ///         <see cref="DistributedCache" /> RefreshAllPublishedSnapshot method.
    ///     </para>
    /// </remarks>
    void Rebuild(
        IReadOnlyCollection<int>? contentTypeIds = null,
        IReadOnlyCollection<int>? mediaTypeIds = null,
        IReadOnlyCollection<int>? memberTypeIds = null);


    /// <summary>
    ///     Rebuilds all internal database caches (but does not reload).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Forces the snapshot service to rebuild its internal database caches. For instance, some caches
    ///         may rely on a database table to store pre-serialized version of documents.
    ///     </para>
    ///     <para>
    ///         This does *not* reload the caches. Caches need to be reloaded, for instance via
    ///         <see cref="DistributedCache" /> RefreshAllPublishedSnapshot method.
    ///     </para>
    /// </remarks>
    void RebuildAll() => Rebuild(Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>());

    /* An IPublishedCachesService implementation can rely on transaction-level events to update
     * its internal, database-level data, as these events are purely internal. However, it cannot
     * rely on cache refreshers CacheUpdated events to update itself, as these events are external
     * and the order-of-execution of the handlers cannot be guaranteed, which means that some
     * user code may run before Umbraco is finished updating itself. Instead, the cache refreshers
     * explicitly notify the service of changes.
     *
     */

    /// <summary>
    ///     Notifies of content cache refresher changes.
    /// </summary>
    /// <param name="payloads">The changes.</param>
    /// <param name="draftChanged">A value indicating whether draft contents have been changed in the cache.</param>
    /// <param name="publishedChanged">A value indicating whether published contents have been changed in the cache.</param>
    void Notify(ContentCacheRefresher.JsonPayload[] payloads, out bool draftChanged, out bool publishedChanged);

    /// <summary>
    ///     Notifies of media cache refresher changes.
    /// </summary>
    /// <param name="payloads">The changes.</param>
    /// <param name="anythingChanged">A value indicating whether medias have been changed in the cache.</param>
    void Notify(MediaCacheRefresher.JsonPayload[] payloads, out bool anythingChanged);

    // there is no NotifyChanges for MemberCacheRefresher because we're not caching members.

    /// <summary>
    ///     Notifies of content type refresher changes.
    /// </summary>
    /// <param name="payloads">The changes.</param>
    void Notify(ContentTypeCacheRefresher.JsonPayload[] payloads);

    /// <summary>
    ///     Notifies of data type refresher changes.
    /// </summary>
    /// <param name="payloads">The changes.</param>
    void Notify(DataTypeCacheRefresher.JsonPayload[] payloads);

    /// <summary>
    ///     Notifies of domain refresher changes.
    /// </summary>
    /// <param name="payloads">The changes.</param>
    void Notify(DomainCacheRefresher.JsonPayload[] payloads);

    /// <summary>
    ///     Cleans up unused snapshots
    /// </summary>
    Task CollectAsync();
}
