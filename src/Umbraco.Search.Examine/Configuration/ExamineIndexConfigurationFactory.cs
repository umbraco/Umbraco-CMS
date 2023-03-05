using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Search.Configuration;

namespace Umbraco.Search.Examine.Configuration;

public class ExamineIndexConfigurationFactory : IExamineIndexConfigurationFactory
{
    private readonly Dictionary<string, IUmbracoIndexConfiguration> configurationObjects;

    public ExamineIndexConfigurationFactory(IPublicAccessService publicAccessService, IScopeProvider provider)
    {
        configurationObjects = new Dictionary<string, IUmbracoIndexConfiguration>()
        {
            { Constants.UmbracoIndexes.InternalIndexName, new UmbracoIndexConfig(publicAccessService, provider)  },
            { Constants.UmbracoIndexes.ExternalIndexName, new UmbracoIndexConfig(publicAccessService, provider) { PublishedValuesOnly = true}},
            { Constants.UmbracoIndexes.MembersIndexName, new UmbracoIndexConfig(publicAccessService, provider) }
        };
    }

    public IUmbracoIndexesConfiguration GetConfiguration()
    {
        return new UmbracoIndexesConfiguration(configurationObjects);
    }
}
