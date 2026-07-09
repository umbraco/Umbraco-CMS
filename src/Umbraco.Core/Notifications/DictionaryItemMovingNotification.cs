// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before dictionary items are moved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the move operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class DictionaryItemMovingNotification : MovingNotification<IDictionaryItem>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryItemMovingNotification"/> class
    ///     with a single move operation.
    /// </summary>
    /// <param name="target">Information about the dictionary item being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DictionaryItemMovingNotification(MoveEventInfo<IDictionaryItem> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryItemMovingNotification"/> class
    ///     with multiple move operations.
    /// </summary>
    /// <param name="target">Information about the dictionary items being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DictionaryItemMovingNotification(IEnumerable<MoveEventInfo<IDictionaryItem>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
