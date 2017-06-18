using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class NotificationMethodSettingsElement : ConfigurationElement
    {
        private const string KEY_KEY = "key";
        private const string VALUE_KEY = "value";

        [ConfigurationProperty(KEY_KEY, IsKey = true, IsRequired = true)]
        public string Key 
        {
            get
            {
                return (string)base[KEY_KEY];
            }
        }

        [ConfigurationProperty(VALUE_KEY, IsRequired = true)]
        public string Value
        {
            get
            {
                return (string)base[VALUE_KEY];
            }
        }
    }
}
