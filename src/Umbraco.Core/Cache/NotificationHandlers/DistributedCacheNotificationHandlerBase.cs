using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TNotification">The type of the notification.</typeparam>
public abstract class DistributedCacheNotificationHandlerBase<TEntity, TNotification> : IDistributedCacheNotificationHandler<TNotification>
    where TNotification : INotification
{
    /// <inheritdoc />
    public void Handle(TNotification notification)
        => Handle(GetEntities(notification));

    /// <inheritdoc />
    public void Handle(IEnumerable<TNotification> notifications)
        => Handle(notifications.SelectMany(GetEntities));

    /// <summary>
    /// Gets the entities from the specified notification.
    /// </summary>
    /// <param name="notification">The notification.</param>
    /// <returns>
    /// The entities.
    /// </returns>
    protected abstract IEnumerable<TEntity> GetEntities(TNotification notification);

    /// <summary>
    /// Handles the specified entities.
    /// </summary>
    /// <param name="entities">The entities.</param>
    protected abstract void Handle(IEnumerable<TEntity> entities);
}
