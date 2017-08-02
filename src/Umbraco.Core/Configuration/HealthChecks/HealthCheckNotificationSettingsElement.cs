using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class HealthCheckNotificationSettingsElement : ConfigurationElement, IHealthCheckNotificationSettingsElement
    {
        private const string ENABLED_KEY = "enabled";
        private const string FIRST_RUN_TIME_KEY = "firstRunTime";
        private const string PERIOD_KEY = "periodInHours";
        private const string NOTIFICATION_METHODS_KEY = "notificationMethods";
        private const string DISABLED_CHECKS_KEY = "disabledChecks";

        [ConfigurationProperty(ENABLED_KEY, IsRequired = true)]
        public bool Enabled
        {
            get
            {
                return (bool)base[ENABLED_KEY];
            }
        }

        [ConfigurationProperty(FIRST_RUN_TIME_KEY, IsRequired = false)]
        public string FirstRunTime
        {
            get
            {
                return (string)base[FIRST_RUN_TIME_KEY];
            }
        }

        [ConfigurationProperty(PERIOD_KEY, IsRequired = true)]
        public int PeriodInHours
        {
            get
            {
                return (int)base[PERIOD_KEY];
            }
        }

        [ConfigurationProperty(NOTIFICATION_METHODS_KEY, IsDefaultCollection = true, IsRequired = false)]
        public NotificationMethodsElementCollection NotificationMethods
        {
            get
            {
                return (NotificationMethodsElementCollection)base[NOTIFICATION_METHODS_KEY];
            }
        }

        [ConfigurationProperty(DISABLED_CHECKS_KEY, IsDefaultCollection = false, IsRequired = false)]
        public DisabledHealthChecksElementCollection DisabledChecks
        {
            get
            {
                return (DisabledHealthChecksElementCollection)base[DISABLED_CHECKS_KEY];
            }
        }
    }
}
