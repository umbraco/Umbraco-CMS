namespace Umbraco.Search.Configuration;

public interface IUmbracoIndexConfiguration
{
    bool PublishedValuesOnly { get; set; }
    bool EnableDefaultEventHandler { get; set; }
}
