namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IWebRoutingSection : IUmbracoConfigurationSection
    {
        bool TrySkipIisCustomErrors { get; }

        bool InternalRedirectPreservesTemplate { get; }

        string UrlProviderMode { get; }
    }

}