namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IWebRoutingSection : IUmbracoConfigurationSection
    {
        bool TrySkipIisCustomErrors { get; }

        bool InternalRedirectPreservesTemplate { get; }

        bool DisableAlternativeTemplates { get; }

        bool DisableFindContentByIdPath { get; }

        string UrlProviderMode { get; }
    }

}