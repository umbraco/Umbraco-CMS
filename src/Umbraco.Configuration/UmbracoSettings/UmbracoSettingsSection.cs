using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class UmbracoSettingsSection : ConfigurationSection, IUmbracoSettingsSection
    {
        [ConfigurationProperty("backOffice")]
        public BackOfficeElement BackOffice => (BackOfficeElement)this["backOffice"];

        [ConfigurationProperty("content")]
        public ContentElement Content => (ContentElement)this["content"];

        [ConfigurationProperty("security")]
        public SecurityElement Security => (SecurityElement)this["security"];

        [ConfigurationProperty("requestHandler")]
        public RequestHandlerElement RequestHandler => (RequestHandlerElement)this["requestHandler"];

        [ConfigurationProperty("logging")]
        public LoggingElement Logging => (LoggingElement)this["logging"];


        [ConfigurationProperty("web.routing")]
        public WebRoutingElement WebRouting => (WebRoutingElement)this["web.routing"];

        IContentSection IUmbracoSettingsSection.Content => Content;

        ISecuritySection IUmbracoSettingsSection.Security => Security;

        IRequestHandlerSection IUmbracoSettingsSection.RequestHandler => RequestHandler;

        IBackOfficeSection IUmbracoSettingsSection.BackOffice => BackOffice;

        ILoggingSection IUmbracoSettingsSection.Logging => Logging;

        IWebRoutingSection IUmbracoSettingsSection.WebRouting => WebRouting;
    }
}
