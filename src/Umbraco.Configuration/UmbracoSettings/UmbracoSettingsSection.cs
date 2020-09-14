using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class UmbracoSettingsSection : ConfigurationSection
    {

        [ConfigurationProperty("content")]
        public ContentElement Content => (ContentElement)this["content"];

        [ConfigurationProperty("security")]
        public SecurityElement Security => (SecurityElement)this["security"];

        [ConfigurationProperty("requestHandler")]
        public RequestHandlerElement RequestHandler => (RequestHandlerElement)this["requestHandler"];

        [ConfigurationProperty("web.routing")]
        public WebRoutingElement WebRouting => (WebRoutingElement)this["web.routing"];

    }
}
