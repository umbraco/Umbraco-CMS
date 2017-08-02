using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class HealthCheckNotificationSettingsElement : ConfigurationElement, IHealthCheckNotificationSettingsElement
    {
        private const string EnabledKey = "enabled";
        private const string FirstRunTimeKey = "firstRunTime";
        private const string PeriodKey = "periodInHours";
        private const string NotificationMethodsKey = "notificationMethods";
        private const string DisabledChecksKey = "disabledChecks";

        [ConfigurationProperty(EnabledKey, IsRequired = true)]
        public bool Enabled
        {
            get
            {
                return (bool)base[EnabledKey];
            }
        }

        [ConfigurationProperty(FirstRunTimeKey, IsRequired = false)]
        public string FirstRunTime
        {
            get
            {
                return (string)base[FirstRunTimeKey];
            }
        }

        [ConfigurationProperty(PeriodKey, IsRequired = true)]
        public int PeriodInHours
        {
            get
            {
                return (int)base[PeriodKey];
            }
        }

        [ConfigurationProperty(NotificationMethodsKey, IsDefaultCollection = true, IsRequired = false)]
        public NotificationMethodsElementCollection NotificationMethods
        {
            get
            {
                return (NotificationMethodsElementCollection)base[NotificationMethodsKey];
            }
        }

        [ConfigurationProperty(DisabledChecksKey, IsDefaultCollection = false, IsRequired = false)]
        public DisabledHealthChecksElementCollection DisabledChecks
        {
            get
            {
                return (DisabledHealthChecksElementCollection)base[DisabledChecksKey];
            }
        }
    }
}
