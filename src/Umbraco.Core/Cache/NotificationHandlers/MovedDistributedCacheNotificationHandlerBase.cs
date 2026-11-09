using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TNotification">The type of the notification.</typeparam>
public abstract class MovedDistributedCacheNotificationHandlerBase<TEntity, TNotification> : DistributedCacheNotificationHandlerBase<MoveEventInfo<TEntity>, TNotification>
    where TNotification : MovedNotification<TEntity>
{
    /// <inheritdoc />
    protected override IEnumerable<MoveEventInfo<TEntity>> GetEntities(TNotification notification)
        => notification.MoveInfoCollection;
}
