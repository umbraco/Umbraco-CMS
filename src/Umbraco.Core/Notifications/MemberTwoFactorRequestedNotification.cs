// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when a member requests two-factor authentication.
/// </summary>
/// <remarks>
///     This notification is published when a member attempts to authenticate and two-factor
///     authentication is required. Handlers can use this for logging or additional security checks.
/// </remarks>
public class MemberTwoFactorRequestedNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTwoFactorRequestedNotification"/> class.
    /// </summary>
    /// <param name="memberKey">The unique key of the member requesting two-factor authentication, or null if not identified.</param>
    public MemberTwoFactorRequestedNotification(Guid? memberKey) => MemberKey = memberKey;

    /// <summary>
    ///     Gets the unique key of the member requesting two-factor authentication, or null if the member is not identified.
    /// </summary>
    public Guid? MemberKey { get; }
}
