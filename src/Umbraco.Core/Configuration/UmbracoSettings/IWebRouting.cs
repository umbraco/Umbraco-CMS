namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IWebRouting
    {
        bool TrySkipIisCustomErrors { get; }

        bool InternalRedirectPreservesTemplate { get; }
    }
}