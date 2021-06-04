namespace Umbraco.Cms.Core.Models.Email
{
    /// <summary>
    /// Represents an email address used for notifications. Contains both the address and its display name.
    /// </summary>
    public class NotificationEmailAddress
    {
        public string DisplayName { get; }

        public string Adress { get; }

        public NotificationEmailAddress(string address, string displayName)
        {
            Adress = address;
            DisplayName = displayName;
        }
    }
}
