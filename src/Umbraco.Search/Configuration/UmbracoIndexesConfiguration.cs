namespace Umbraco.Search.Configuration;

public class UmbracoIndexesConfiguration : IUmbracoIndexesConfiguration
{
    private readonly Dictionary<string, IUmbracoIndexConfiguration> configurationObjects;

    /// <summary>
    ///
    /// </summary>
    /// <param name="configuration"></param>
    public UmbracoIndexesConfiguration(Dictionary<string, IUmbracoIndexConfiguration> configuration)
    {
        configurationObjects = configuration;
    }

    public IUmbracoIndexConfiguration Configuration(string name)
    {
        return configurationObjects[name];
    }
}
