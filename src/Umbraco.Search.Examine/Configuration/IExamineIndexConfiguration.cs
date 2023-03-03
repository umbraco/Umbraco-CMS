namespace Umbraco.Search.Examine.Configuration;

public interface IExamineIndexConfiguration
{
    IUmbracoExamineIndexConfig Configuration(string name);
}