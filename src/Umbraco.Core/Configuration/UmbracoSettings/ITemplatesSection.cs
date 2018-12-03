using System;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface ITemplatesSection : IUmbracoConfigurationSection
    {
        RenderingEngine DefaultRenderingEngine { get; }
    }
}
