namespace Umbraco.Search.Configuration;

public interface IUmbracoIndexesConfiguration
{
    IUmbracoIndexConfiguration Configuration(string name);
}
