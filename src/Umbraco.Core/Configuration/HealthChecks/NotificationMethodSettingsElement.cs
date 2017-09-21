using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class NotificationMethodSettingsElement : ConfigurationElement, INotificationMethodSettings
    {
        private const string KeyKey = "key";
        private const string ValueKey = "value";

        [ConfigurationProperty(KeyKey, IsKey = true, IsRequired = true)]
        public string Key 
        {
            get
            {
                return (string)base[KeyKey];
            }
        }

        [ConfigurationProperty(ValueKey, IsRequired = true)]
        public string Value
        {
            get
            {
                return (string)base[ValueKey];
            }
        }
    }
}
