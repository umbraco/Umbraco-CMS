using System;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class TemplatesElement : UmbracoConfigurationElement, ITemplatesSection
    {
        [ConfigurationProperty("useAspNetMasterPages")]
        internal InnerTextConfigurationElement<bool> UseAspNetMasterPages
        {
            get { return GetOptionalTextElement("useAspNetMasterPages", true); }
        }

        [ConfigurationProperty("enableSkinSupport")]
        internal InnerTextConfigurationElement<bool> EnableSkinSupport
        {
            get { return GetOptionalTextElement("enableSkinSupport", true); }
        }

        [ConfigurationProperty("defaultRenderingEngine", IsRequired = true)]
        internal InnerTextConfigurationElement<RenderingEngine> DefaultRenderingEngine
        {
            get { return GetOptionalTextElement("defaultRenderingEngine", RenderingEngine.Mvc); }
        }

        [Obsolete("This has no affect and will be removed in future versions")]
        [ConfigurationProperty("enableTemplateFolders")]
        internal InnerTextConfigurationElement<bool> EnableTemplateFolders
        {
            get { return GetOptionalTextElement("enableTemplateFolders", false); }
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