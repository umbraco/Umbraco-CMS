namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IKeepAliveSection : IUmbracoConfigurationSection
    {
        bool DisableKeepAliveTask { get; }
        string KeepAlivePingUrl { get; }
    }
}
