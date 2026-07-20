// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when a user requests a password reset via the "forgot password" flow.
/// </summary>
/// <remarks>
///     This notification is published when a user initiates the password reset process,
///     before the reset email is sent.
/// </remarks>
public class UserForgotPasswordRequestedNotification : UserNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserForgotPasswordRequestedNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address of the user requesting the password reset.</param>
    /// <param name="affectedUserId">The ID of the user requesting the password reset.</param>
    /// <param name="performingUserId">The ID of the user performing the request.</param>
    public UserForgotPasswordRequestedNotification(string ipAddress, string affectedUserId, string performingUserId)
        : base(ipAddress, affectedUserId, performingUserId)
    {
    }
}
