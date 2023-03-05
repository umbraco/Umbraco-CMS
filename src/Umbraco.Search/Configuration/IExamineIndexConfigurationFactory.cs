namespace Umbraco.Search.Examine.Configuration;

public interface IExamineIndexConfigurationFactory
{
    public IUmbracoIndexesConfiguration GetConfiguration();
}
