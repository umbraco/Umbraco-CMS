using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Represents the entry point into Umbraco's distributed cache infrastructure.
/// </summary>
/// <remarks>
///     <para>
///         The distributed cache infrastructure ensures that distributed caches are
///         invalidated properly in load balancing environments.
///     </para>
///     <para>
///         Distribute caches include static (in-memory) cache, runtime cache, front-end content cache, Examine/Lucene
///         indexes
///     </para>
/// </remarks>
public sealed class DistributedCache
{
    private readonly CacheRefresherCollection _cacheRefreshers;
    private readonly IServerMessenger _serverMessenger;

    public DistributedCache(IServerMessenger serverMessenger, CacheRefresherCollection cacheRefreshers)
    {
        _serverMessenger = serverMessenger;
        _cacheRefreshers = cacheRefreshers;
    }

    #region Core notification methods

    /// <summary>
    ///     Notifies the distributed cache of specified item invalidation, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <typeparam name="T">The type of the invalidated items.</typeparam>
    /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
    /// <param name="getNumericId">A function returning the unique identifier of items.</param>
    /// <param name="instances">The invalidated items.</param>
    /// <remarks>
    ///     This method is much better for performance because it does not need to re-lookup object instances.
    /// </remarks>
    public void Refresh<T>(Guid refresherGuid, Func<T, int> getNumericId, params T[] instances)
    {
        if (refresherGuid == Guid.Empty || instances.Length == 0 || getNumericId == null)
        {
            return;
        }

        _serverMessenger.QueueRefresh(
            GetRefresherById(refresherGuid),
            getNumericId,
            instances);
    }

    // helper method to get an ICacheRefresher by its unique identifier
    private ICacheRefresher GetRefresherById(Guid refresherGuid)
    {
        ICacheRefresher? refresher = _cacheRefreshers[refresherGuid];
        if (refresher == null)
        {
            throw new InvalidOperationException($"No cache refresher found with id {refresherGuid}");
        }

        return refresher;
    }

    /// <summary>
    ///     Notifies the distributed cache of a specified item invalidation, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
    /// <param name="id">The unique identifier of the invalidated item.</param>
    public void Refresh(Guid refresherGuid, int id)
    {
        if (refresherGuid == Guid.Empty || id == default)
        {
            return;
        }

        _serverMessenger.QueueRefresh(
            GetRefresherById(refresherGuid),
            id);
    }

    /// <summary>
    ///     Notifies the distributed cache of a specified item invalidation, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
    /// <param name="id">The unique identifier of the invalidated item.</param>
    public void Refresh(Guid refresherGuid, Guid id)
    {
        if (refresherGuid == Guid.Empty || id == Guid.Empty)
        {
            return;
        }

        _serverMessenger.QueueRefresh(
            GetRefresherById(refresherGuid),
            id);
    }

    // payload should be an object, or array of objects, NOT a
    // Linq enumerable of some sort (IEnumerable, query...)
    public void RefreshByPayload<TPayload>(Guid refresherGuid, TPayload[] payload)
    {
        if (refresherGuid == Guid.Empty || payload == null)
        {
            return;
        }

        _serverMessenger.QueueRefresh(
            GetRefresherById(refresherGuid),
            payload);
    }

    // so deal with IEnumerable
    public void RefreshByPayload<TPayload>(Guid refresherGuid, IEnumerable<TPayload> payloads)
        where TPayload : class
    {
        if (refresherGuid == Guid.Empty || payloads == null)
        {
            return;
        }

        _serverMessenger.QueueRefresh(
            GetRefresherById(refresherGuid),
            payloads.ToArray());
    }

    ///// <summary>
    ///// Notifies the distributed cache, for a specified <see cref="ICacheRefresher"/>.
    ///// </summary>
    ///// <param name="refresherId">The unique identifier of the ICacheRefresher.</param>
    ///// <param name="payload">The notification content.</param>
    // internal void Notify(Guid refresherId, object payload)
    // {
    //    if (refresherId == Guid.Empty || payload == null) return;

    // _serverMessenger.Notify(
    //        Current.ServerRegistrar.Registrations,
    //        GetRefresherById(refresherId),
    //        json);
    // }

    /// <summary>
    ///     Notifies the distributed cache of a global invalidation for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
    public void RefreshAll(Guid refresherGuid)
    {
        if (refresherGuid == Guid.Empty)
        {
            return;
        }

        _serverMessenger.QueueRefreshAll(
            GetRefresherById(refresherGuid));
    }

    /// <summary>
    ///     Notifies the distributed cache of a specified item removal, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
    /// <param name="id">The unique identifier of the removed item.</param>
    public void Remove(Guid refresherGuid, int id)
    {
        if (refresherGuid == Guid.Empty || id == default)
        {
            return;
        }

        _serverMessenger.QueueRemove(
            GetRefresherById(refresherGuid),
            id);
    }

    /// <summary>
    ///     Notifies the distributed cache of specified item removal, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <typeparam name="T">The type of the removed items.</typeparam>
    /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
    /// <param name="getNumericId">A function returning the unique identifier of items.</param>
    /// <param name="instances">The removed items.</param>
    /// <remarks>
    ///     This method is much better for performance because it does not need to re-lookup object instances.
    /// </remarks>
    public void Remove<T>(Guid refresherGuid, Func<T, int> getNumericId, params T[] instances) =>
        _serverMessenger.QueueRemove(
            GetRefresherById(refresherGuid),
            getNumericId,
            instances);

    #endregion
}
