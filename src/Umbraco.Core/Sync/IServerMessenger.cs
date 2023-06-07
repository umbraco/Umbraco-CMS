using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core.Sync;

/// <summary>
///     Transmits distributed cache notifications for all servers of a load balanced environment.
/// </summary>
/// <remarks>Also ensures that the notification is processed on the local environment.</remarks>
public interface IServerMessenger
{
    /// <summary>
    ///     Called to synchronize a server with queued notifications
    /// </summary>
    void Sync();

    /// <summary>
    ///     Called to send/commit the queued messages created with the Perform methods
    /// </summary>
    void SendMessages();

    /// <summary>
    ///     Notifies the distributed cache, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <param name="refresher">The ICacheRefresher.</param>
    /// <param name="payload">The notification content.</param>
    void QueueRefresh<TPayload>(ICacheRefresher refresher, TPayload[] payload);

    /// <summary>
    ///     Notifies the distributed cache of specified item invalidation, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <typeparam name="T">The type of the invalidated items.</typeparam>
    /// <param name="refresher">The ICacheRefresher.</param>
    /// <param name="getNumericId">A function returning the unique identifier of items.</param>
    /// <param name="instances">The invalidated items.</param>
    void QueueRefresh<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances);

    /// <summary>
    ///     Notifies the distributed cache of specified item invalidation, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <typeparam name="T">The type of the invalidated items.</typeparam>
    /// <param name="refresher">The ICacheRefresher.</param>
    /// <param name="getGuidId">A function returning the unique identifier of items.</param>
    /// <param name="instances">The invalidated items.</param>
    void QueueRefresh<T>(ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances);

    /// <summary>
    ///     Notifies all servers of specified items removal, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <typeparam name="T">The type of the removed items.</typeparam>
    /// <param name="refresher">The ICacheRefresher.</param>
    /// <param name="getNumericId">A function returning the unique identifier of items.</param>
    /// <param name="instances">The removed items.</param>
    void QueueRemove<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances);

    /// <summary>
    ///     Notifies all servers of specified items removal, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <param name="refresher">The ICacheRefresher.</param>
    /// <param name="numericIds">The unique identifiers of the removed items.</param>
    void QueueRemove(ICacheRefresher refresher, params int[] numericIds);

    /// <summary>
    ///     Notifies all servers of specified items invalidation, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <param name="refresher">The ICacheRefresher.</param>
    /// <param name="numericIds">The unique identifiers of the invalidated items.</param>
    void QueueRefresh(ICacheRefresher refresher, params int[] numericIds);

    /// <summary>
    ///     Notifies all servers of specified items invalidation, for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <param name="refresher">The ICacheRefresher.</param>
    /// <param name="guidIds">The unique identifiers of the invalidated items.</param>
    void QueueRefresh(ICacheRefresher refresher, params Guid[] guidIds);

    /// <summary>
    ///     Notifies all servers of a global invalidation for a specified <see cref="ICacheRefresher" />.
    /// </summary>
    /// <param name="refresher">The ICacheRefresher.</param>
    void QueueRefreshAll(ICacheRefresher refresher);
}
