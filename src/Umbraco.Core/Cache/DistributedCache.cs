using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Represents the entry point into Umbraco's distributed cache infrastructure.
/// </summary>
/// <remarks>
/// <para>
/// The distributed cache infrastructure ensures that distributed caches are invalidated properly in load balancing environments.
/// </para>
/// <para>
/// Distribute caches include static (in-memory) cache, runtime cache, front-end content cache and Examine/Lucene indexes.
/// indexes
/// </para>
/// </remarks>
public sealed class DistributedCache
{
    private readonly IServerMessenger _serverMessenger;
    private readonly CacheRefresherCollection _cacheRefreshers;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedCache" /> class.
    /// </summary>
    /// <param name="serverMessenger">The server messenger.</param>
    /// <param name="cacheRefreshers">The cache refreshers.</param>
    public DistributedCache(IServerMessenger serverMessenger, CacheRefresherCollection cacheRefreshers)
    {
        _serverMessenger = serverMessenger;
        _cacheRefreshers = cacheRefreshers;
    }

    /// <summary>
    /// Notifies the distributed cache of specified item invalidation, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <typeparam name="T">The type of the invalidated items.</typeparam>
    /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
    /// <param name="getNumericId">A function returning the unique identifier of items.</param>
    /// <param name="instances">The invalidated items.</param>
    /// <remarks>
    /// This method is much better for performance because it does not need to re-lookup object instances.
    /// </remarks>
    public void Refresh<T>(Guid refresherGuid, Func<T, int> getNumericId, params T[] instances)
    {
        if (refresherGuid == Guid.Empty || getNumericId is null || instances is null || instances.Length == 0)
        {
            return;
        }

        _serverMessenger.QueueRefresh(GetRefresherById(refresherGuid), getNumericId, instances);
    }

    // helper method to get an ICacheRefresher by its unique identifier
    private ICacheRefresher GetRefresherById(Guid refresherGuid)
        => _cacheRefreshers[refresherGuid] ?? throw new InvalidOperationException($"No cache refresher found with id {refresherGuid}");

    /// <summary>
    /// Notifies the distributed cache of a specified item invalidation, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
    /// <param name="id">The unique identifier of the invalidated item.</param>
    public void Refresh(Guid refresherGuid, int id)
    {
        if (refresherGuid == Guid.Empty || id == default)
        {
            return;
        }

        _serverMessenger.QueueRefresh(GetRefresherById(refresherGuid), id);
    }

    /// <summary>
    /// Notifies the distributed cache of a specified item invalidation, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
    /// <param name="id">The unique identifier of the invalidated item.</param>
    public void Refresh(Guid refresherGuid, Guid id)
    {
        if (refresherGuid == Guid.Empty || id == Guid.Empty)
        {
            return;
        }

        _serverMessenger.QueueRefresh(GetRefresherById(refresherGuid), id);
    }

    /// <summary>
    /// Refreshes the distributed cache by payload.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload.</typeparam>
    /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
    /// <param name="payload">The payload.</param>
    /// <remarks>
    /// The payload should be an object, or array of objects, NOT a LINQ enumerable of some sort (IEnumerable, query...).
    /// </remarks>
    public void RefreshByPayload<TPayload>(Guid refresherGuid, TPayload[] payload)
    {
        if (refresherGuid == Guid.Empty || payload is null || payload.Length == 0)
        {
            return;
        }

        _serverMessenger.QueueRefresh(GetRefresherById(refresherGuid), payload);
    }

    /// <summary>
    /// Refreshes the distributed cache by payload.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload.</typeparam>
    /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
    /// <param name="payloads">The payloads.</param>
    public void RefreshByPayload<TPayload>(Guid refresherGuid, IEnumerable<TPayload> payloads)
        where TPayload : class
        => RefreshByPayload(refresherGuid, payloads.ToArray());

    /// <summary>
    /// Notifies the distributed cache of a global invalidation for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
    public void RefreshAll(Guid refresherGuid)
    {
        if (refresherGuid == Guid.Empty)
        {
            return;
        }

        _serverMessenger.QueueRefreshAll(GetRefresherById(refresherGuid));
    }

    /// <summary>
    /// Notifies the distributed cache of a specified item removal, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
    /// <param name="id">The unique identifier of the removed item.</param>
    public void Remove(Guid refresherGuid, int id)
    {
        if (refresherGuid == Guid.Empty || id == default)
        {
            return;
        }

        _serverMessenger.QueueRemove(GetRefresherById(refresherGuid), id);
    }

    /// <summary>
    /// Notifies the distributed cache of specified item removal, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <typeparam name="T">The type of the removed items.</typeparam>
    /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
    /// <param name="getNumericId">A function returning the unique identifier of items.</param>
    /// <param name="instances">The removed items.</param>
    /// <remarks>
    /// This method is much better for performance because it does not need to re-lookup object instances.
    /// </remarks>
    public void Remove<T>(Guid refresherGuid, Func<T, int> getNumericId, params T[] instances)
    {
        if (refresherGuid == Guid.Empty || getNumericId is null || instances is null || instances.Length == 0)
        {
            return;
        }

        _serverMessenger.QueueRemove(GetRefresherById(refresherGuid), getNumericId, instances);
    }
}
