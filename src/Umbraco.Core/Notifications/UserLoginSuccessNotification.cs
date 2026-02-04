// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a user has successfully logged in.
/// </summary>
/// <remarks>
///     This notification is useful for audit logging and security monitoring purposes.
/// </remarks>
public class UserLoginSuccessNotification : UserNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserLoginSuccessNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address of the user logging in.</param>
    /// <param name="affectedUserId">The ID of the user who logged in.</param>
    /// <param name="performingUserId">The ID of the user performing the login action.</param>
    public UserLoginSuccessNotification(string ipAddress, string affectedUserId, string performingUserId)
        : base(ipAddress, affectedUserId, performingUserId)
    {
    }
}
