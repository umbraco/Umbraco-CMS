// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before member groups are deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class MemberGroupDeletingNotification : DeletingNotification<IMemberGroup>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberGroupDeletingNotification"/> class
    ///     with a single member group.
    /// </summary>
    /// <param name="target">The member group being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberGroupDeletingNotification(IMemberGroup target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberGroupDeletingNotification"/> class
    ///     with multiple member groups.
    /// </summary>
    /// <param name="target">The member groups being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberGroupDeletingNotification(IEnumerable<IMemberGroup> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
