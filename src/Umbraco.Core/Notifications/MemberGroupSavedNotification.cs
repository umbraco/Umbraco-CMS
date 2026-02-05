// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after member groups have been saved.
/// </summary>
/// <remarks>
///     This notification is published after member groups have been successfully saved,
///     allowing handlers to react for auditing or cache invalidation purposes.
/// </remarks>
public class MemberGroupSavedNotification : SavedNotification<IMemberGroup>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberGroupSavedNotification"/> class
    ///     with a single member group.
    /// </summary>
    /// <param name="target">The member group that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberGroupSavedNotification(IMemberGroup target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberGroupSavedNotification"/> class
    ///     with multiple member groups.
    /// </summary>
    /// <param name="target">The member groups that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberGroupSavedNotification(IEnumerable<IMemberGroup> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
