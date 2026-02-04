// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before user groups are saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public sealed class UserGroupSavingNotification : SavingNotification<IUserGroup>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupSavingNotification"/> class
    ///     with a single user group.
    /// </summary>
    /// <param name="target">The user group being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserGroupSavingNotification(IUserGroup target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupSavingNotification"/> class
    ///     with multiple user groups.
    /// </summary>
    /// <param name="target">The user groups being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserGroupSavingNotification(IEnumerable<IUserGroup> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
