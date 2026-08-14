// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for member authentication notifications such as login, logout, and login failure.
/// </summary>
public abstract class MemberNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address of the member performing the action.</param>
    /// <param name="memberKey">The key of the member affected by this action.</param>
    protected MemberNotification(string ipAddress, Guid? memberKey)
    {
        DateTimeUtc = DateTime.UtcNow;
        IpAddress = ipAddress;
        MemberKey = memberKey;
    }

    /// <summary>
    ///     Gets the date and time in UTC when this notification was created.
    /// </summary>
    public DateTime DateTimeUtc { get; }

    /// <summary>
    ///     Gets the source IP address of the member performing the action.
    /// </summary>
    public string IpAddress { get; }

    /// <summary>
    ///     Gets the key of the member affected by this action, or <c>null</c> if the member could not be found.
    /// </summary>
    public Guid? MemberKey { get; }
}
