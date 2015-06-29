using System;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface ITemplatesSection : IUmbracoConfigurationSection
    {
        bool UseAspNetMasterPages { get; }

        bool EnableSkinSupport { get; }

        RenderingEngine DefaultRenderingEngine { get; }

        [Obsolete("This has no affect and will be removed in future versions")]
        bool EnableTemplateFolders { get; }
    }
}