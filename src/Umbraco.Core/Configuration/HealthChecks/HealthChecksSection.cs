using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class HealthChecksSection : ConfigurationSection
    {
        private const string DISABLED_CHECKS_KEY = "disabledChecks";
        private const string NOTIFICATION_SETTINGS_KEY = "notificationSettings";

        [ConfigurationProperty(DISABLED_CHECKS_KEY)]
        public DisabledHealthChecksElementCollection DisabledChecks
        {
            get { return ((DisabledHealthChecksElementCollection)(base[DISABLED_CHECKS_KEY])); }
        }

        [ConfigurationProperty(NOTIFICATION_SETTINGS_KEY, IsRequired = true)]
        public HealthCheckNotificationSettingsElement NotificationSettings
        {
            get { return ((HealthCheckNotificationSettingsElement)(base[NOTIFICATION_SETTINGS_KEY])); }
        }
    }
}
