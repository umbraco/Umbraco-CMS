// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications that can carry state between "ing" (before) and "ed" (after) notifications.
/// </summary>
/// <remarks>
///     This class implements <see cref="IStatefulNotification"/> and provides a dictionary for storing
///     custom state data that persists between paired notifications (e.g., ContentSaving and ContentSaved).
/// </remarks>
public abstract class StatefulNotification : IStatefulNotification
{
    private IDictionary<string, object?>? _state;

    /// <summary>
    ///     Gets or sets a dictionary for storing custom state data.
    /// </summary>
    /// <remarks>
    ///     This can be used by event subscribers to store state in the notification so they easily deal with custom state data
    ///     between a starting ("ing") and an ending ("ed") notification.
    /// </remarks>
    public IDictionary<string, object?> State
    {
        get => _state ??= new Dictionary<string, object?>();
        set => _state = value;
    }
}
