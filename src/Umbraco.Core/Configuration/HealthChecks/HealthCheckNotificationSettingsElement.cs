using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class HealthCheckNotificationSettingsElement : ConfigurationElement
    {
        private const string ENABLED_KEY = "enabled";
        private const string FIRST_RUN_TIME_KEY = "firstRunTime";
        private const string PERIOD_KEY = "periodInHours";
        private const string RECIPIENT_EMAIL_KEY = "recipientEmail";
        private const string WEBHOOK_URL_KEY = "webHookUrl";
        private const string DISABLED_CHECKS_KEY = "disabledChecks";

        [ConfigurationProperty(ENABLED_KEY, IsRequired = true)]
        public bool Enabled
        {
            get
            {
                return ((bool)(base[ENABLED_KEY]));
            }
        }

        [ConfigurationProperty(FIRST_RUN_TIME_KEY, IsRequired = false)]
        public string FirstRunTime
        {
            get
            {
                return ((string)(base[FIRST_RUN_TIME_KEY]));
            }
        }

        [ConfigurationProperty(PERIOD_KEY, IsRequired = true)]
        public int PeriodInHours
        {
            get
            {
                return ((int)(base[PERIOD_KEY]));
            }
        }

        [ConfigurationProperty(RECIPIENT_EMAIL_KEY, IsRequired = true)]
        public string RecipientEmail
        {
            get
            {
                return ((string)(base[RECIPIENT_EMAIL_KEY]));
            }
        }

        [ConfigurationProperty(WEBHOOK_URL_KEY, IsRequired = true)]
        public string WebhookUrl
        {
            get
            {
                return ((string)(base[WEBHOOK_URL_KEY]));
            }
        }

        [ConfigurationProperty(DISABLED_CHECKS_KEY, IsDefaultCollection = true, IsRequired = false)]
        public DisabledHealthChecksElementCollection DisabledChecks
        {
            get
            {
                return ((DisabledHealthChecksElementCollection)(base[DISABLED_CHECKS_KEY]));
            }
        }
    }
}
