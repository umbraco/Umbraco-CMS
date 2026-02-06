// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when a user's password has been changed via the "forgot password" flow.
/// </summary>
/// <remarks>
///     This notification is published after a user has successfully changed their password
///     through the password reset mechanism.
/// </remarks>
public class UserForgotPasswordChangedNotification : UserNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserForgotPasswordChangedNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address of the user changing the password.</param>
    /// <param name="affectedUserId">The ID of the user whose password was changed.</param>
    /// <param name="performingUserId">The ID of the user performing the password change.</param>
    public UserForgotPasswordChangedNotification(string ipAddress, string affectedUserId, string performingUserId)
        : base(ipAddress, affectedUserId, performingUserId)
    {
    }
}
