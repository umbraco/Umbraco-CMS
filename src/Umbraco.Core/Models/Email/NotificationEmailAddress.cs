namespace Umbraco.Cms.Core.Models.Email;

/// <summary>
///     Represents an email address used for notifications. Contains both the address and its display name.
/// </summary>
public class NotificationEmailAddress
{
    public NotificationEmailAddress(string address, string displayName)
    {
        Address = address;
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    public string Address { get; }
}
