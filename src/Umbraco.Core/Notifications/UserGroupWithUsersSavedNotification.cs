// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after user groups with their associated users have been saved.
/// </summary>
/// <remarks>
///     This notification includes both the user group and the users assigned to it.
/// </remarks>
public sealed class UserGroupWithUsersSavedNotification : SavedNotification<UserGroupWithUsers>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupWithUsersSavedNotification"/> class
    ///     with a single user group with users.
    /// </summary>
    /// <param name="target">The user group with users that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserGroupWithUsersSavedNotification(UserGroupWithUsers target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupWithUsersSavedNotification"/> class
    ///     with multiple user groups with users.
    /// </summary>
    /// <param name="target">The user groups with users that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserGroupWithUsersSavedNotification(IEnumerable<UserGroupWithUsers> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
