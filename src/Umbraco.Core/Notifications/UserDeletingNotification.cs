// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before a user is deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation.
///     The notification is published by the <see cref="Services.IUserService"/> before the user is removed.
/// </remarks>
public sealed class UserDeletingNotification : DeletingNotification<IUser>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserDeletingNotification"/> class with a single user.
    /// </summary>
    /// <param name="target">The user being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserDeletingNotification(IUser target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserDeletingNotification"/> class with multiple users.
    /// </summary>
    /// <param name="target">The collection of users being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserDeletingNotification(IEnumerable<IUser> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
