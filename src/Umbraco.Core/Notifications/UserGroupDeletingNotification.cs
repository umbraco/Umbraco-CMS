// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before user groups are deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public sealed class UserGroupDeletingNotification : DeletingNotification<IUserGroup>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupDeletingNotification"/> class
    ///     with a single user group.
    /// </summary>
    /// <param name="target">The user group being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserGroupDeletingNotification(IUserGroup target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupDeletingNotification"/> class
    ///     with multiple user groups.
    /// </summary>
    /// <param name="target">The user groups being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserGroupDeletingNotification(IEnumerable<IUserGroup> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
