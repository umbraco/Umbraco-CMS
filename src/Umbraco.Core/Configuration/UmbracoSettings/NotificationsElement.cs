using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class NotificationsElement : ConfigurationElement
    {
        [ConfigurationProperty("email")]
        internal InnerTextConfigurationElement<string> NotificationEmailAddress
        {
            get { return (InnerTextConfigurationElement<string>)this["email"]; }
        }

        [ConfigurationProperty("disableHtmlEmail")]
        internal InnerTextConfigurationElement<bool> DisableHtmlEmail
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>) this["disableHtmlEmail"], 
                    //set the default
                    false);
            }
        }

    }
}