namespace Umbraco.Configuration.Models
{
    public class ContentNotificationSettings
    {
        public string NotificationEmailAddress { get; set; }

        public bool DisableHtmlEmail { get; set; } = false;
    }
}
