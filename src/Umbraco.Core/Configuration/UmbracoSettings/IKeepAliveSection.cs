namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IKeepAliveSection : IUmbracoConfigurationSection
    {
        bool EnableKeepAliveTask { get; }
        string KeepAlivePingUrl { get; }
    }
}
