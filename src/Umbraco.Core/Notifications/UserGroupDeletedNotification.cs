// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after user groups have been deleted.
/// </summary>
/// <remarks>
///     This notification is published after user groups have been successfully deleted,
///     allowing handlers to react for auditing or cache invalidation purposes.
/// </remarks>
public sealed class UserGroupDeletedNotification : DeletedNotification<IUserGroup>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupDeletedNotification"/> class
    ///     with a single user group.
    /// </summary>
    /// <param name="target">The user group that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserGroupDeletedNotification(IUserGroup target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupDeletedNotification"/> class
    ///     with multiple user groups.
    /// </summary>
    /// <param name="target">The user groups that were deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserGroupDeletedNotification(IEnumerable<IUserGroup> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
