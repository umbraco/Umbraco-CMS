namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IInternalSearchFieldsToSearchSection : IUmbracoConfigurationSection
    {
        string ContentSearchFields { get; }
    }
}
