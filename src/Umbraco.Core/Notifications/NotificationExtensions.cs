// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Provides extension methods for notifications to support state transfer.
/// </summary>
public static class NotificationExtensions
{
    /// <summary>
    ///     Sets the state dictionary on a stateful notification.
    /// </summary>
    /// <typeparam name="T">The type of notification.</typeparam>
    /// <param name="notification">The notification to set state on.</param>
    /// <param name="state">The state dictionary to set.</param>
    /// <returns>The notification with state set, for method chaining.</returns>
    public static T WithState<T>(this T notification, IDictionary<string, object?>? state)
        where T : IStatefulNotification
    {
        notification.State = state!;
        return notification;
    }

    /// <summary>
    ///     Copies the state from a source notification to the target notification.
    /// </summary>
    /// <typeparam name="T">The type of target notification.</typeparam>
    /// <typeparam name="TSource">The type of source notification.</typeparam>
    /// <param name="notification">The notification to set state on.</param>
    /// <param name="source">The source notification to copy state from.</param>
    /// <returns>The notification with state copied, for method chaining.</returns>
    public static T WithStateFrom<T, TSource>(this T notification, TSource source)
        where T : IStatefulNotification
        where TSource : IStatefulNotification
        => notification.WithState(source.State);
}
