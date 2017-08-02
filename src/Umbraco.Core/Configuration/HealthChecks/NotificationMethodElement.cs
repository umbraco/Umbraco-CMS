using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class NotificationMethodElement : ConfigurationElement, INotificationMethodElement
    {
        private const string AliasKey = "alias";
        private const string EnabledKey = "enabled";
        private const string VerbosityKey = "verbosity";
        private const string FailureonlyKey = "failureOnly";
        private const string SettingsKey = "settings";

        [ConfigurationProperty(AliasKey, IsKey = true, IsRequired = true)]
        public string Alias 
        {
            get
            {
                return (string)base[AliasKey];
            }
        }

        [ConfigurationProperty(EnabledKey, IsKey = true, IsRequired = true)]
        public bool Enabled
        {
            get
            {
                return (bool)base[EnabledKey];
            }
        }

        [ConfigurationProperty(VerbosityKey, IsRequired = true)]
        public HealthCheckNotificationVerbosity Verbosity
        {
            get
            {
                return (HealthCheckNotificationVerbosity)base[VerbosityKey];
            }
        }

        [ConfigurationProperty(FailureonlyKey, IsRequired = false)]
        public bool FailureOnly
        {
            get
            {
                return (bool)base[FailureonlyKey];
            }
        }

        [ConfigurationProperty(SettingsKey, IsDefaultCollection = true, IsRequired = false)]
        public NotificationMethodSettingsElementCollection Settings
        {
            get
            {
                return (NotificationMethodSettingsElementCollection)base[SettingsKey];
            }
        }
    }
}
