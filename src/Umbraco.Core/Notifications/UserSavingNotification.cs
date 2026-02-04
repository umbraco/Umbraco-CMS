// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before a user is saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation.
///     The notification is published by the <see cref="Services.IUserService"/> before the user is persisted.
/// </remarks>
public sealed class UserSavingNotification : SavingNotification<IUser>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserSavingNotification"/> class with a single user.
    /// </summary>
    /// <param name="target">The user being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserSavingNotification(IUser target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserSavingNotification"/> class with multiple users.
    /// </summary>
    /// <param name="target">The collection of users being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserSavingNotification(IEnumerable<IUser> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
