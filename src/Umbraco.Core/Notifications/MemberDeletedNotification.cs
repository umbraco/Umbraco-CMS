// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a member has been deleted.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IMemberService"/> when Delete or DeleteMembersOfType methods complete.
///     It is not cancelable since the delete operation has already completed.
/// </remarks>
public sealed class MemberDeletedNotification : DeletedNotification<IMember>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberDeletedNotification"/> class with a single member.
    /// </summary>
    /// <param name="target">The member that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberDeletedNotification(IMember target, EventMessages messages)
        : base(target, messages)
    {
    }
}
