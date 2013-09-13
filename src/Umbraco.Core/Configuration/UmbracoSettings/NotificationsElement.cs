using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class NotificationsElement : ConfigurationElement, INotifications
    {
        [ConfigurationProperty("email")]
        internal InnerTextConfigurationElement<string> EmailAddress
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

        string INotifications.EmailAddress
        {
            get { return EmailAddress; }
        }

        bool INotifications.DisableHtmlEmail
        {
            get { return DisableHtmlEmail; }
        }
    }
}