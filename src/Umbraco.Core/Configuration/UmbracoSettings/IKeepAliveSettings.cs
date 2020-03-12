namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IKeepAliveSettings : IUmbracoConfigurationSection
    {
        bool DisableKeepAliveTask { get; }
        string KeepAlivePingUrl { get; }
    }
}
