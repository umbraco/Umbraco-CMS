using System;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class TemplatesElement : ConfigurationElement, ITemplatesSection
    {
        [ConfigurationProperty("useAspNetMasterPages")]
        internal InnerTextConfigurationElement<bool> UseAspNetMasterPages
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>)this["useAspNetMasterPages"],
                    //set the default
                    true);
            }
        }

        [ConfigurationProperty("enableSkinSupport")]
        internal InnerTextConfigurationElement<bool> EnableSkinSupport
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>)this["enableSkinSupport"],
                    //set the default
                    true);
            }
        }

        [ConfigurationProperty("defaultRenderingEngine", IsRequired = true)]
        internal InnerTextConfigurationElement<RenderingEngine> DefaultRenderingEngine
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<RenderingEngine>(
                    (InnerTextConfigurationElement<RenderingEngine>)this["defaultRenderingEngine"],
                    //set the default
                    RenderingEngine.Mvc);
            }
        }

        [Obsolete("This has no affect and will be removed in future versions")]
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

        bool ITemplatesSection.UseAspNetMasterPages
        {
            get { return UseAspNetMasterPages; }
        }

        bool ITemplatesSection.EnableSkinSupport
        {
            get { return EnableSkinSupport; }
        }

        RenderingEngine ITemplatesSection.DefaultRenderingEngine
        {
            get { return DefaultRenderingEngine; }
        }

        [Obsolete("This has no affect and will be removed in future versions")]
        bool ITemplatesSection.EnableTemplateFolders
        {
            get { return EnableTemplateFolders; }
        }
    }
}