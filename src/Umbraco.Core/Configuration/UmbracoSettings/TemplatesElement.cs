using System;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class TemplatesElement : UmbracoConfigurationElement, ITemplatesSection
    {
        [ConfigurationProperty("defaultRenderingEngine", IsRequired = true)]
        internal InnerTextConfigurationElement<RenderingEngine> DefaultRenderingEngine
        {
            get { return GetOptionalTextElement("defaultRenderingEngine", RenderingEngine.Mvc); }
        }
        
        RenderingEngine ITemplatesSection.DefaultRenderingEngine
        {
            get { return DefaultRenderingEngine; }
        }

    }
}
