using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class NotificationsElement : UmbracoConfigurationElement
    {
        [ConfigurationProperty("email")]
        internal InnerTextConfigurationElement<string> NotificationEmailAddress => (InnerTextConfigurationElement<string>)this["email"];

        [ConfigurationProperty("disableHtmlEmail")]
        internal InnerTextConfigurationElement<bool> DisableHtmlEmail => GetOptionalTextElement("disableHtmlEmail", false);
    }
}
