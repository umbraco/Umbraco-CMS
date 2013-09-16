namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface ITemplatesSection : IUmbracoConfigurationSection
    {
        bool UseAspNetMasterPages { get; }

        bool EnableSkinSupport { get; }

        RenderingEngine DefaultRenderingEngine { get; }

        bool EnableTemplateFolders { get; }
    }
}