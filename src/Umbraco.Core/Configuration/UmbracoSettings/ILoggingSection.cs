namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface ILoggingSection : IUmbracoConfigurationSection
    {
        int MaxLogAge { get; }
    }
}
