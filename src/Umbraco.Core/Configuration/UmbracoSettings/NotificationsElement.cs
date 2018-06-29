using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class NotificationsElement : UmbracoConfigurationElement
    {
        [ConfigurationProperty("email")]
        internal InnerTextConfigurationElement<string> NotificationEmailAddress
        {
            get { return (InnerTextConfigurationElement<string>)this["email"]; }
        }

        [ConfigurationProperty("disableHtmlEmail")]
        internal InnerTextConfigurationElement<bool> DisableHtmlEmail
        {
            get { return GetOptionalTextElement("disableHtmlEmail", false); }
        }

    }
}
