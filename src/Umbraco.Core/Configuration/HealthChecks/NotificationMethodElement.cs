using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class NotificationMethodElement : ConfigurationElement, INotificationMethodElement
    {
        private const string ALIAS_KEY = "alias";
        private const string ENABLED_KEY = "enabled";
        private const string VERBOSITY_KEY = "verbosity";
        private const string FAILUREONLY_KEY = "failureOnly";
        private const string SETTINGS_KEY = "settings";

        [ConfigurationProperty(ALIAS_KEY, IsKey = true, IsRequired = true)]
        public string Alias 
        {
            get
            {
                return (string)base[ALIAS_KEY];
            }
        }

        [ConfigurationProperty(ENABLED_KEY, IsKey = true, IsRequired = true)]
        public bool Enabled
        {
            get
            {
                return (bool)base[ENABLED_KEY];
            }
        }

        [ConfigurationProperty(VERBOSITY_KEY, IsRequired = true)]
        public HealthCheckNotificationVerbosity Verbosity
        {
            get
            {
                return (HealthCheckNotificationVerbosity)base[VERBOSITY_KEY];
            }
        }

        [ConfigurationProperty(FAILUREONLY_KEY, IsRequired = false)]
        public bool FailureOnly
        {
            get
            {
                return (bool)base[FAILUREONLY_KEY];
            }
        }

        [ConfigurationProperty(SETTINGS_KEY, IsDefaultCollection = true, IsRequired = false)]
        public NotificationMethodSettingsElementCollection Settings
        {
            get
            {
                return (NotificationMethodSettingsElementCollection)base[SETTINGS_KEY];
            }
        }
    }
}
