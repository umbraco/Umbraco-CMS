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
    ///     <para>
    ///         Use this property to pass data between a starting ("ing") and an ending ("ed") notification handler.
    ///     </para>
    ///     <para>
    ///         A single starting notification can be followed by several ending notifications that all share its dictionary
    ///         instance, for example one for each entity affected by the operation. Where stored data relates to individual
    ///         entities, key it by entity, and don't assume every entity in an ending notification has an entry.
    ///     </para>
    /// </remarks>
    IDictionary<string, object?> State { get; set; }
}
