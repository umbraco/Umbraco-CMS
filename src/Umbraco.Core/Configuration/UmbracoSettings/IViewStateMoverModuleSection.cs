namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IViewStateMoverModuleSection : IUmbracoConfigurationSection
    {
        bool Enable { get; }
    }
}