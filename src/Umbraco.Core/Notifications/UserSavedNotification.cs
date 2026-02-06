// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a user has been saved.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IUserService"/> after the user has been persisted.
///     It is not cancelable since the save operation has already completed.
/// </remarks>
public sealed class UserSavedNotification : SavedNotification<IUser>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserSavedNotification"/> class with a single user.
    /// </summary>
    /// <param name="target">The user that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserSavedNotification(IUser target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserSavedNotification"/> class with multiple users.
    /// </summary>
    /// <param name="target">The collection of users that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserSavedNotification(IEnumerable<IUser> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
