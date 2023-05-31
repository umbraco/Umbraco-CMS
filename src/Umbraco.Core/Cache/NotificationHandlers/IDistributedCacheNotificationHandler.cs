using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Defines a handler that triggers a distributed cache refresher.
/// </summary>
public interface IDistributedCacheNotificationHandler
{
    /// <summary>
    /// Handles the specified notifications.
    /// </summary>
    /// <param name="notifications">The notifications.</param>
    void Handle(IEnumerable<INotification> notifications);
}

/// <summary>
/// Defines a handler for a <typeparamref name="TNotification" /> that triggers a distributed cache refresher.
/// </summary>
/// <typeparam name="TNotification">The type of the notification.</typeparam>
public interface IDistributedCacheNotificationHandler<in TNotification> : INotificationHandler<TNotification>, IDistributedCacheNotificationHandler
    where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notifications.
    /// </summary>
    /// <param name="notifications">The notifications.</param>
    /// <remarks>
    /// Handling multiple notifications in a single action avoids creating multiple cache refresher operations and improves performance.
    /// </remarks>
    void Handle(IEnumerable<TNotification> notifications);

    /// <inheritdoc />
    void IDistributedCacheNotificationHandler.Handle(IEnumerable<INotification> notifications)
        => Handle(notifications.OfType<TNotification>());
}
