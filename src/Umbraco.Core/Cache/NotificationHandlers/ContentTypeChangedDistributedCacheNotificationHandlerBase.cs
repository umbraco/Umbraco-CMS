using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TNotification">The type of the notification.</typeparam>
/// <typeparam name="TNotificationObject">The type of the notification object.</typeparam>
public abstract class ContentTypeChangedDistributedCacheNotificationHandlerBase<TEntity, TNotification, TNotificationObject> : DistributedCacheNotificationHandlerBase<TEntity, TNotification>
    where TNotificationObject : class, IContentTypeComposition
    where TNotification : ContentTypeChangeNotification<TNotificationObject>
{ }

/// <inheritdoc />
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TNotification">The type of the notification.</typeparam>
public abstract class ContentTypeChangedDistributedCacheNotificationHandlerBase<TEntity, TNotification> : ContentTypeChangedDistributedCacheNotificationHandlerBase<ContentTypeChange<TEntity>, TNotification, TEntity>
    where TEntity : class, IContentTypeComposition
    where TNotification : ContentTypeChangeNotification<TEntity>
{
    /// <inheritdoc />
    protected override IEnumerable<ContentTypeChange<TEntity>> GetEntities(TNotification notification)
        => notification.Changes;
}
