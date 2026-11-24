// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Records the starting ("ing") and ending ("ed") notifications of an operation, so tests can verify that state written
///     in a starting notification reaches the ending notifications.
/// </summary>
internal sealed class NotificationStateRecorder
{
    public const string StateKey = "NotificationStateTest";

    public List<IStatefulNotification> Starting { get; } = [];

    public List<IStatefulNotification> Ending { get; } = [];
}

/// <summary>
///     Writes a unique value into the state of a starting notification and records the notification.
/// </summary>
internal sealed class NotificationStateWritingHandler<TNotification>(NotificationStateRecorder recorder)
    : INotificationHandler<TNotification>
    where TNotification : IStatefulNotification
{
    public void Handle(TNotification notification)
    {
        notification.State[NotificationStateRecorder.StateKey] = Guid.NewGuid();
        recorder.Starting.Add(notification);
    }
}

/// <summary>
///     Records an ending notification so its state can be inspected.
/// </summary>
internal sealed class NotificationStateReadingHandler<TNotification>(NotificationStateRecorder recorder)
    : INotificationHandler<TNotification>
    where TNotification : IStatefulNotification
{
    public void Handle(TNotification notification) => recorder.Ending.Add(notification);
}
