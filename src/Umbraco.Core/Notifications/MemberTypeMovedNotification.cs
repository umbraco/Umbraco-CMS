// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after member types have been moved.
/// </summary>
/// <remarks>
///     This notification is published after member types have been successfully moved
///     to a new parent in the member type tree.
/// </remarks>
public class MemberTypeMovedNotification : MovedNotification<IMemberType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeMovedNotification"/> class
    ///     with a single move operation.
    /// </summary>
    /// <param name="target">Information about the member type that was moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberTypeMovedNotification(MoveEventInfo<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeMovedNotification"/> class
    ///     with multiple move operations.
    /// </summary>
    /// <param name="target">Information about the member types that were moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberTypeMovedNotification(IEnumerable<MoveEventInfo<IMemberType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
