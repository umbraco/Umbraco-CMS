// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after dictionary items have been moved.
/// </summary>
/// <remarks>
///     This notification is published after dictionary items have been successfully moved
///     to a new parent in the dictionary tree.
/// </remarks>
public class DictionaryItemMovedNotification : MovedNotification<IDictionaryItem>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryItemMovedNotification"/> class
    ///     with a single move operation.
    /// </summary>
    /// <param name="target">Information about the dictionary item that was moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DictionaryItemMovedNotification(MoveEventInfo<IDictionaryItem> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryItemMovedNotification"/> class
    ///     with multiple move operations.
    /// </summary>
    /// <param name="target">Information about the dictionary items that were moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DictionaryItemMovedNotification(IEnumerable<MoveEventInfo<IDictionaryItem>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
