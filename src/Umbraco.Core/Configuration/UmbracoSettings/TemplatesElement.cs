using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class TemplatesElement : ConfigurationElement
    {
        [ConfigurationProperty("useAspNetMasterPages")]
        internal InnerTextConfigurationElement<bool> UseAspNetMasterPages
        {
            get { return (InnerTextConfigurationElement<bool>)this["useAspNetMasterPages"]; }
        }

        [ConfigurationProperty("enableSkinSupport")]
        internal InnerTextConfigurationElement<bool> EnableSkinSupport
        {
            get { return (InnerTextConfigurationElement<bool>)this["enableSkinSupport"]; }
        }

        [ConfigurationProperty("defaultRenderingEngine")]
        internal InnerTextConfigurationElement<RenderingEngine> DefaultRenderingEngine
        {
            get { return (InnerTextConfigurationElement<RenderingEngine>)this["defaultRenderingEngine"]; }
        }

        [ConfigurationProperty("enableTemplateFolders")]
        internal InnerTextConfigurationElement<bool> EnableTemplateFolders
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>)this["enableTemplateFolders"],
                    //set the default
                    false);
            }
        }
    }
}