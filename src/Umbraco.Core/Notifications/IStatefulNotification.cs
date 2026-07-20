// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Represents a notification that can carry state between "ing" (before) and "ed" (after) notifications.
/// </summary>
/// <remarks>
///     This interface allows notification handlers to store custom state data that persists
///     between the starting notification (e.g., ContentSaving) and the ending notification (e.g., ContentSaved).
/// </remarks>
public interface IStatefulNotification : INotification
{
    /// <summary>
    ///     Gets or sets a dictionary for storing custom state data.
    /// </summary>
    /// <remarks>
    ///     Use this property to pass data between a starting ("ing") and an ending ("ed") notification handler.
    /// </remarks>
    IDictionary<string, object?> State { get; set; }
}
