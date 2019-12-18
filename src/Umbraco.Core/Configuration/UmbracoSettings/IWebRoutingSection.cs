namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IWebRoutingSection : IUmbracoConfigurationSection
    {
        bool TrySkipIisCustomErrors { get; }

        bool InternalRedirectPreservesTemplate { get; }

        bool DisableAlternativeTemplates { get; }

        bool ValidateAlternativeTemplates { get; }

        bool DisableFindContentByIdPath { get; }

        bool DisableRedirectUrlTracking { get; }

        string UrlProviderMode { get; }

        string UmbracoApplicationUrl { get; }
    }
}
