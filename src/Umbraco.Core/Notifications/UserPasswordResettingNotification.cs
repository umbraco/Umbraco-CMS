// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before a user's password is reset.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the password reset
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class UserPasswordResettingNotification : CancelableObjectNotification<IUser>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserPasswordResettingNotification"/> class.
    /// </summary>
    /// <param name="target">The user whose password is being reset.</param>
    /// <param name="messages">The event messages collection.</param>
    public UserPasswordResettingNotification(IUser target, EventMessages messages) : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the user whose password is being reset.
    /// </summary>
    public IUser User => Target;
}
