using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public class UmbracoSettingsSection : ConfigurationSection, IUmbracoSettingsSection
    {
        [ConfigurationProperty("backOffice")]
        internal BackOfficeElement BackOffice
        {
            get { return (BackOfficeElement)this["backOffice"]; }
        }

        [ConfigurationProperty("content")]
        internal ContentElement Content
        {
            get { return (ContentElement)this["content"]; }
        }

        [ConfigurationProperty("security")]
        internal SecurityElement Security
        {
            get { return (SecurityElement)this["security"]; }
        }

        [ConfigurationProperty("requestHandler")]
        internal RequestHandlerElement RequestHandler
        {
            get { return (RequestHandlerElement)this["requestHandler"]; }
        }

        [ConfigurationProperty("logging")]
        internal LoggingElement Logging
        {
            get { return (LoggingElement)this["logging"]; }
        }

        [ConfigurationProperty("scheduledTasks")]
        internal ScheduledTasksElement ScheduledTasks
        {
            get { return (ScheduledTasksElement)this["scheduledTasks"]; }
        }

        [ConfigurationProperty("providers")]
        internal ProvidersElement Providers
        {
            get { return (ProvidersElement)this["providers"]; }
        }

        [ConfigurationProperty("web.routing")]
        internal WebRoutingElement WebRouting
        {
            get { return (WebRoutingElement)this["web.routing"]; }
        }

        IContentSection IUmbracoSettingsSection.Content
        {
            get { return Content; }
        }

        ISecuritySection IUmbracoSettingsSection.Security
        {
            get { return Security; }
        }

        IRequestHandlerSection IUmbracoSettingsSection.RequestHandler
        {
            get { return RequestHandler; }
        }

        IBackOfficeSection IUmbracoSettingsSection.BackOffice
        {
            get { return BackOffice; }
        }

        ILoggingSection IUmbracoSettingsSection.Logging
        {
            get { return Logging; }
        }

        IScheduledTasksSection IUmbracoSettingsSection.ScheduledTasks
        {
            get { return ScheduledTasks; }
        }
        
        IProvidersSection IUmbracoSettingsSection.Providers
        {
            get { return Providers; }
        }

        IWebRoutingSection IUmbracoSettingsSection.WebRouting
        {
            get { return WebRouting; }
        }
    }
}
