using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     A base class for cache refreshers that handles events.
/// </summary>
/// <remarks>The actual cache refresher type is used for strongly typed events.</remarks>
public abstract class CacheRefresherBase<TNotification> : ICacheRefresher
    where TNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CacheRefresherBase{TInstanceType}" />.
    /// </summary>
    protected CacheRefresherBase(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
    {
        AppCaches = appCaches;
        EventAggregator = eventAggregator;
        NotificationFactory = factory;
    }

    #region Define

    /// <summary>
    ///     Gets the unique identifier of the refresher.
    /// </summary>
    public abstract Guid RefresherUniqueId { get; }

    /// <summary>
    ///     Gets the name of the refresher.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    ///     Gets the <see cref="ICacheRefresherNotificationFactory" /> for <see cref="TNotification" />
    /// </summary>
    protected ICacheRefresherNotificationFactory NotificationFactory { get; }

    #endregion

    #region Refresher

    /// <summary>
    ///     Refreshes all entities.
    /// </summary>
    public virtual void RefreshAll() =>

        // NOTE: We pass in string.Empty here because if we pass in NULL this causes problems with
        // the underlying ActivatorUtilities.CreateInstance which doesn't seem to support passing in
        // null to an 'object' parameter and we end up with "A suitable constructor for type 'ZYZ' could not be located."
        // In this case, all cache refreshers should be checking for the type first before checking for a msg value
        // so this shouldn't cause any issues.
        OnCacheUpdated(NotificationFactory.Create<TNotification>(string.Empty, MessageType.RefreshAll));

    /// <summary>
    ///     Refreshes an entity.
    /// </summary>
    /// <param name="id">The entity's identifier.</param>
    public virtual void Refresh(int id) =>
        OnCacheUpdated(NotificationFactory.Create<TNotification>(id, MessageType.RefreshById));

    /// <summary>
    ///     Refreshes an entity.
    /// </summary>
    /// <param name="id">The entity's identifier.</param>
    public virtual void Refresh(Guid id) =>
        OnCacheUpdated(NotificationFactory.Create<TNotification>(id, MessageType.RefreshById));

    /// <summary>
    ///     Removes an entity.
    /// </summary>
    /// <param name="id">The entity's identifier.</param>
    public virtual void Remove(int id) =>
        OnCacheUpdated(NotificationFactory.Create<TNotification>(id, MessageType.RemoveById));

    #endregion

    #region Protected

    /// <summary>
    ///     Gets the cache helper.
    /// </summary>
    protected AppCaches AppCaches { get; }

    protected IEventAggregator EventAggregator { get; }

    /// <summary>
    ///     Clears the cache for all repository entities of a specified type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities.</typeparam>
    protected void ClearAllIsolatedCacheByEntityType<TEntity>()
        where TEntity : class, IEntity =>
        AppCaches.IsolatedCaches.ClearCache<TEntity>();

    /// <summary>
    ///     Raises the CacheUpdated event.
    /// </summary>
    protected void OnCacheUpdated(CacheRefresherNotification notification) => EventAggregator.Publish(notification);

    #endregion
}
