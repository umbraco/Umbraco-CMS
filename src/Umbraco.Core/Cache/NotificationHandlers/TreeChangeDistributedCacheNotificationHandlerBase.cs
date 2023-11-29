using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TNotification">The type of the notification.</typeparam>
/// <typeparam name="TNotificationObject">The type of the notification object.</typeparam>
public abstract class TreeChangeDistributedCacheNotificationHandlerBase<TEntity, TNotification, TNotificationObject> : DistributedCacheNotificationHandlerBase<TEntity, TNotification>
    where TNotification : TreeChangeNotification<TNotificationObject>
{ }

/// <inheritdoc />
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TNotification">The type of the notification.</typeparam>
public abstract class TreeChangeDistributedCacheNotificationHandlerBase<TEntity, TNotification> : TreeChangeDistributedCacheNotificationHandlerBase<TreeChange<TEntity>, TNotification, TEntity>
    where TNotification : TreeChangeNotification<TEntity>
{
    /// <inheritdoc />
    protected override IEnumerable<TreeChange<TEntity>> GetEntities(TNotification notification)
        => notification.Changes;
}
