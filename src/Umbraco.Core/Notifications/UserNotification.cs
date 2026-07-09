// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for user-related notifications such as login, logout, password changes, and locking.
/// </summary>
/// <remarks>
///     This class provides common properties for tracking the user action, the affected user,
///     and the performing user for audit and security logging purposes.
/// </remarks>
public abstract class UserNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address of the user performing the action.</param>
    /// <param name="affectedUserId">The ID of the user affected by this action.</param>
    /// <param name="performingUserId">The ID of the user performing this action.</param>
    protected UserNotification(string ipAddress, string? affectedUserId, string performingUserId)
    {
        DateTimeUtc = DateTime.UtcNow;
        IpAddress = ipAddress;
        AffectedUserId = affectedUserId;
        PerformingUserId = performingUserId;
    }

    /// <summary>
    ///     Gets the date and time in UTC when this notification was created.
    /// </summary>
    public DateTime DateTimeUtc { get; }

    /// <summary>
    ///     Gets the source IP address of the user performing the action.
    /// </summary>
    public string IpAddress { get; }

    /// <summary>
    ///     Gets the ID of the user affected by this action.
    /// </summary>
    public string? AffectedUserId { get; }

    /// <summary>
    ///     Gets the ID of the user performing this action.
    /// </summary>
    /// <remarks>
    ///     If a user is performing an action on a different user, this will differ from <see cref="AffectedUserId"/>.
    /// </remarks>
    public string PerformingUserId { get; }
}
