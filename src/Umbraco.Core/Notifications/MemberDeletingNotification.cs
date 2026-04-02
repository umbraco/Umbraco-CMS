// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before a member is deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation.
///     The notification is published by the <see cref="Services.IMemberService"/> when Delete or DeleteMembersOfType methods are called.
/// </remarks>
public sealed class MemberDeletingNotification : DeletingNotification<IMember>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberDeletingNotification"/> class with a single member.
    /// </summary>
    /// <param name="target">The member being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberDeletingNotification(IMember target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberDeletingNotification"/> class with multiple members.
    /// </summary>
    /// <param name="target">The collection of members being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberDeletingNotification(IEnumerable<IMember> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
