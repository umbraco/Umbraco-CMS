using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Marker interface for notification handlers that triggers a distributed cache refresher.
/// </summary>
public interface IDistributedCacheNotificationHandler : INotificationHandler
{ }

/// <summary>
/// Defines a handler for a <typeparamref name="TNotification" /> that triggers a distributed cache refresher.
/// </summary>
/// <typeparam name="TNotification">The type of the notification.</typeparam>
public interface IDistributedCacheNotificationHandler<in TNotification> : INotificationHandler<TNotification>, IDistributedCacheNotificationHandler
    where TNotification : INotification
{ }
