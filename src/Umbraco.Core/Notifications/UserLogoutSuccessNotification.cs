// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a user has successfully logged out.
/// </summary>
/// <remarks>
///     This notification is useful for audit logging and implementing custom sign-out redirect behavior.
/// </remarks>
public class UserLogoutSuccessNotification : UserNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserLogoutSuccessNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address of the user logging out.</param>
    /// <param name="affectedUserId">The ID of the user who logged out.</param>
    /// <param name="performingUserId">The ID of the user performing the logout action.</param>
    public UserLogoutSuccessNotification(string ipAddress, string? affectedUserId, string performingUserId)
        : base(ipAddress, affectedUserId, performingUserId)
    {
    }

    /// <summary>
    ///     Gets or sets the URL to redirect to after sign-out.
    /// </summary>
    /// <remarks>
    ///     Handlers can set this property to customize the redirect URL after the user logs out.
    /// </remarks>
    public string? SignOutRedirectUrl { get; set; }
}
