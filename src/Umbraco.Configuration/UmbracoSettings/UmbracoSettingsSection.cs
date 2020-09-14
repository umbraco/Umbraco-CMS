using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class UmbracoSettingsSection : ConfigurationSection
    {

        [ConfigurationProperty("content")]
        public ContentElement Content => (ContentElement)this["content"];


    }
}
