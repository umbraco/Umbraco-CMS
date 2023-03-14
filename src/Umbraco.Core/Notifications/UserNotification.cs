namespace Umbraco.Cms.Core.Notifications;

public abstract class UserNotification : INotification
{
    protected UserNotification(string ipAddress, string? affectedUserId, string performingUserId)
    {
        DateTimeUtc = DateTime.UtcNow;
        IpAddress = ipAddress;
        AffectedUserId = affectedUserId;
        PerformingUserId = performingUserId;
    }

    /// <summary>
    ///     Current date/time in UTC format
    /// </summary>
    public DateTime DateTimeUtc { get; }

    /// <summary>
    ///     The source IP address of the user performing the action
    /// </summary>
    public string IpAddress { get; }

    /// <summary>
    ///     The user affected by the event raised
    /// </summary>
    public string? AffectedUserId { get; }

    /// <summary>
    ///     If a user is performing an action on a different user, then this will be set. Otherwise it will be -1
    /// </summary>
    public string PerformingUserId { get; }
}
