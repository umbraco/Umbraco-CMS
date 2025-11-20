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
        => Handle(
            GetEntities(notification),
            notification is StatefulNotification statefulNotification
                ? statefulNotification.State
                : new Dictionary<string, object?>());

    /// <inheritdoc />
    public void Handle(IEnumerable<TNotification> notifications)
    {
        foreach (TNotification notification in notifications)
        {
            Handle(notification);
        }
    }

    /// <summary>
    /// Gets the entities from the specified notification.
    /// </summary>
    /// <param name="notification">The notification.</param>
    /// <returns>
    /// The entities.
    /// </returns>
    protected abstract IEnumerable<TEntity> GetEntities(TNotification notification);

    // TODO (V18): When removing the obsolete method, make the remaining Handle method abstract
    // rather than virtual. It couldn't be made abstract when introduced as that would be a breaking change.

    /// <summary>
    /// Handles the specified entities.
    /// </summary>
    /// <param name="entities">The entities.</param>
    [Obsolete("Please use the overload taking all parameters. Scheduled for removal in Umbraco 18.")]
    protected abstract void Handle(IEnumerable<TEntity> entities);

    /// <summary>
    /// Handles the specified entities.
    /// </summary>
    /// <param name="entities">The entities.</param>
    /// <param name="state">The notification state.</param>
    protected virtual void Handle(IEnumerable<TEntity> entities, IDictionary<string, object?> state)
#pragma warning disable CS0618 // Type or member is obsolete
        => Handle(entities);
#pragma warning restore CS0618 // Type or member is obsolete
}
