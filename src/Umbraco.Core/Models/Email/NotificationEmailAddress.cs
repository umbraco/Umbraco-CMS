namespace Umbraco.Cms.Core.Models.Email;

/// <summary>
///     Represents an email address used for notifications. Contains both the address and its display name.
/// </summary>
public class NotificationEmailAddress
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationEmailAddress" /> class.
    /// </summary>
    /// <param name="address">The email address.</param>
    /// <param name="displayName">The display name for the email address.</param>
    public NotificationEmailAddress(string address, string displayName)
    {
        Address = address;
        DisplayName = displayName;
    }

    /// <summary>
    ///     Gets the display name for the email address.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    ///     Gets the email address.
    /// </summary>
    public string Address { get; }
}
