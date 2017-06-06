using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class SlackSettingsElement : BaseNotificationMethodElement
    {
        private const string WEBHOOKURL_KEY = "webHookUrl";
        private const string CHANNEL_KEY = "channel";
        private const string USERNAME_KEY = "username";

        [ConfigurationProperty(WEBHOOKURL_KEY, IsRequired = true)]
        public string WebHookUrl
        {
            get
            {
                return ((string)(base[WEBHOOKURL_KEY]));
            }
        }

        [ConfigurationProperty(CHANNEL_KEY, IsRequired = true)]
        public string Channel
        {
            get
            {
                return ((string)(base[CHANNEL_KEY]));
            }
        }

        [ConfigurationProperty(USERNAME_KEY, IsRequired = true)]
        public string UserName
        {
            get
            {
                return ((string)(base[USERNAME_KEY]));
            }
        }
    }
}
