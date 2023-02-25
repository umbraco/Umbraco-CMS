namespace Umbraco.Search.Examine;

public class ExamineIndexConfiguration : IExamineIndexConfiguration
{
    private readonly Dictionary<string, IUmbracoExamineIndexConfig> configurationObjects;

    /// <summary>
    ///
    /// </summary>
    /// <param name="configuration"></param>
    public ExamineIndexConfiguration(Dictionary<string, IUmbracoExamineIndexConfig> configuration)
    {
        configurationObjects = configuration;
    }

    public IUmbracoExamineIndexConfig Configuration(string name)
    {
        return configurationObjects[name];
    }
}

public interface IExamineIndexConfiguration
{
    IUmbracoExamineIndexConfig Configuration(string name);
}
