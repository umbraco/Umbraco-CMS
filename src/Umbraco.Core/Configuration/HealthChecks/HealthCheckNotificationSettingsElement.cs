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
        private const string DISABLED_CHECKS_KEY = "disabledChecks";
        private const string EMAIL_SETTINGS_KEY = "emailSettings";
        private const string SLACK_SETTINGS_KEY = "slackSettings";

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

        [ConfigurationProperty(DISABLED_CHECKS_KEY, IsDefaultCollection = true, IsRequired = false)]
        public DisabledHealthChecksElementCollection DisabledChecks
        {
            get
            {
                return ((DisabledHealthChecksElementCollection)(base[DISABLED_CHECKS_KEY]));
            }
        }

        [ConfigurationProperty(EMAIL_SETTINGS_KEY, IsDefaultCollection = true, IsRequired = false)]
        public EmailSettingsElement EmailSettings
        {
            get
            {
                return ((EmailSettingsElement)(base[EMAIL_SETTINGS_KEY]));
            }
        }

        [ConfigurationProperty(SLACK_SETTINGS_KEY, IsDefaultCollection = true, IsRequired = false)]
        public SlackSettingsElement SlackSettings
        {
            get
            {
                return ((SlackSettingsElement)(base[SLACK_SETTINGS_KEY]));
            }
        }
    }
}
