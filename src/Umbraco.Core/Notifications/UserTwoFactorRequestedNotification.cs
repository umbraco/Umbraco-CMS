// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published when two-factor authentication is requested for a user.
/// </summary>
/// <remarks>
///     This notification can be used to implement custom two-factor authentication providers
///     or to perform additional actions when 2FA is triggered.
/// </remarks>
public class UserTwoFactorRequestedNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserTwoFactorRequestedNotification"/> class.
    /// </summary>
    /// <param name="userKey">The unique key of the user requesting two-factor authentication.</param>
    public UserTwoFactorRequestedNotification(Guid userKey) => UserKey = userKey;

    /// <summary>
    ///     Gets the unique key of the user requesting two-factor authentication.
    /// </summary>
    public Guid UserKey { get; }
}
