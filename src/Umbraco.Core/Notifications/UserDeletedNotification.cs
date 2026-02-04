// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a user has been deleted.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IUserService"/> after the user has been removed.
///     It is not cancelable since the delete operation has already completed.
/// </remarks>
public sealed class UserDeletedNotification : DeletedNotification<IUser>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserDeletedNotification"/> class with a single user.
    /// </summary>
    /// <param name="target">The user that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserDeletedNotification(IUser target, EventMessages messages)
        : base(target, messages)
    {
    }
}
