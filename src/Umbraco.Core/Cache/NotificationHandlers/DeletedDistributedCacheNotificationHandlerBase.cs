using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TNotification">The type of the notification.</typeparam>
/// <typeparam name="TNotificationObject">The type of the notification object.</typeparam>
public abstract class DeletedDistributedCacheNotificationHandlerBase<TEntity, TNotification, TNotificationObject> : DistributedCacheNotificationHandlerBase<TEntity, TNotification>
    where TNotification : DeletedNotification<TNotificationObject>
{ }

/// <inheritdoc />
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TNotification">The type of the notification.</typeparam>
public abstract class DeletedDistributedCacheNotificationHandlerBase<TEntity, TNotification> : DeletedDistributedCacheNotificationHandlerBase<TEntity, TNotification, TEntity>
    where TNotification : DeletedNotification<TEntity>
{
    /// <inheritdoc />
    protected override IEnumerable<TEntity> GetEntities(TNotification notification)
        => notification.DeletedEntities;
}
