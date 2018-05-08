using System;
using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class HealthChecksSection : ConfigurationSection, IHealthChecks
    {
        private const string DisabledChecksKey = "disabledChecks";
        private const string NotificationSettingsKey = "notificationSettings";

        [ConfigurationProperty(DisabledChecksKey)]
        public DisabledHealthChecksElementCollection DisabledChecks
        {
            get { return ((DisabledHealthChecksElementCollection)(base[DisabledChecksKey])); }
        }

        [ConfigurationProperty(NotificationSettingsKey, IsRequired = true)]
        public HealthCheckNotificationSettingsElement NotificationSettings
        {
            get { return ((HealthCheckNotificationSettingsElement)(base[NotificationSettingsKey])); }
        }

        IEnumerable<IDisabledHealthCheck> IHealthChecks.DisabledChecks
        {
            get { return DisabledChecks; }
        }

        IHealthCheckNotificationSettings IHealthChecks.NotificationSettings
        {
            get { return NotificationSettings; }
        }
    }
}
