namespace Umbraco.Core.Configuration.Models
{
    public class ContentNotificationSettings
    {
        public string Email { get; set; }

        public bool DisableHtmlEmail { get; set; } = false;
    }
}
