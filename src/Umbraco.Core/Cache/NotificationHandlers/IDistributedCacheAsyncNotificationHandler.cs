using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Defines an asynchronous handler for a <typeparamref name="TNotification" /> that should be invoked when notifications are dispatched in a distributed cache scope (e.g. to trigger a distributed cache refresher).
/// </summary>
/// <typeparam name="TNotification">The type of the notification.</typeparam>
public interface IDistributedCacheAsyncNotificationHandler<in TNotification> : INotificationAsyncHandler<TNotification>, IDistributedCacheNotificationHandler
    where TNotification : INotification
{ }
