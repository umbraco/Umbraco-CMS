// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before user groups with their associated users are saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public sealed class UserGroupWithUsersSavingNotification : SavingNotification<UserGroupWithUsers>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupWithUsersSavingNotification"/> class
    ///     with a single user group with users.
    /// </summary>
    /// <param name="target">The user group with users being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserGroupWithUsersSavingNotification(UserGroupWithUsers target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupWithUsersSavingNotification"/> class
    ///     with multiple user groups with users.
    /// </summary>
    /// <param name="target">The user groups with users being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserGroupWithUsersSavingNotification(IEnumerable<UserGroupWithUsers> target, EventMessages messages)
        : base(
        target, messages)
    {
    }
}
