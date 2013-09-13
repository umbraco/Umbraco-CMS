namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface ITemplates
    {
        bool UseAspNetMasterPages { get; }

        bool EnableSkinSupport { get; }

        RenderingEngine DefaultRenderingEngine { get; }

        bool EnableTemplateFolders { get; }
    }
}