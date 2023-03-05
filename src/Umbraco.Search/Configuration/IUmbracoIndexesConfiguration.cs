using Umbraco.Search.Configuration;

namespace Umbraco.Search.Examine.Configuration;

public interface IUmbracoIndexesConfiguration
{
    IUmbracoIndexConfiguration Configuration(string name);
}
