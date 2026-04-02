// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after member groups have been deleted.
/// </summary>
/// <remarks>
///     This notification is published after member groups have been successfully deleted,
///     allowing handlers to react for auditing or cache invalidation purposes.
/// </remarks>
public class MemberGroupDeletedNotification : DeletedNotification<IMemberGroup>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberGroupDeletedNotification"/> class.
    /// </summary>
    /// <param name="target">The member group that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberGroupDeletedNotification(IMemberGroup target, EventMessages messages)
        : base(target, messages)
    {
    }
}
