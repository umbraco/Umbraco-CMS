namespace Umbraco.Search.Configuration;

public interface IIndexConfigurationFactory
{
    public IUmbracoIndexesConfiguration GetConfiguration();
}
