using Umbraco.Search.Configuration;

namespace Umbraco.Search.Lifti.Configuration;

public class UmbracoInMemoryIndexConfig : IUmbracoIndexConfiguration
{
    public UmbracoInMemoryIndexConfig(bool publishedValuesOnly = false, bool enableDefaultEventHandler  = true)
    {
        PublishedValuesOnly = publishedValuesOnly;
        EnableDefaultEventHandler = enableDefaultEventHandler;
    }

    public bool PublishedValuesOnly { get; set; }
    public bool EnableDefaultEventHandler { get; set; }
}
