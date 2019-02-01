using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public class UmbracoSettingsSection : ConfigurationSection, IUmbracoSettingsSection
    {
        [ConfigurationProperty("backOffice")]
        internal BackOfficeElement BackOffice => (BackOfficeElement)this["backOffice"];

        [ConfigurationProperty("content")]
        internal ContentElement Content => (ContentElement)this["content"];

        [ConfigurationProperty("security")]
        internal SecurityElement Security => (SecurityElement)this["security"];

        [ConfigurationProperty("requestHandler")]
        internal RequestHandlerElement RequestHandler => (RequestHandlerElement)this["requestHandler"];

        [ConfigurationProperty("logging")]
        internal LoggingElement Logging => (LoggingElement)this["logging"];


        [ConfigurationProperty("web.routing")]
        internal WebRoutingElement WebRouting => (WebRoutingElement)this["web.routing"];

        IContentSection IUmbracoSettingsSection.Content => Content;

        ISecuritySection IUmbracoSettingsSection.Security => Security;

        IRequestHandlerSection IUmbracoSettingsSection.RequestHandler => RequestHandler;

        IBackOfficeSection IUmbracoSettingsSection.BackOffice => BackOffice;

        ILoggingSection IUmbracoSettingsSection.Logging => Logging;

        IWebRoutingSection IUmbracoSettingsSection.WebRouting => WebRouting;
    }
}
