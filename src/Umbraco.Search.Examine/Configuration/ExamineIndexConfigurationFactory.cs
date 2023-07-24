using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Search.Configuration;

namespace Umbraco.Search.Examine.Configuration;

public class IndexConfigurationFactory : IIndexConfigurationFactory
{
    private readonly Dictionary<string, IUmbracoIndexConfiguration> configurationObjects;

    public IndexConfigurationFactory(IPublicAccessService publicAccessService, IScopeProvider provider)
    {
        configurationObjects = new Dictionary<string, IUmbracoIndexConfiguration>()
        {
            { Constants.UmbracoIndexes.InternalIndexName, new UmbracoExamineIndexConfig(publicAccessService, provider)  },
            { Constants.UmbracoIndexes.ExternalIndexName, new UmbracoExamineIndexConfig(publicAccessService, provider) { PublishedValuesOnly = true}},
            { Constants.UmbracoIndexes.MembersIndexName, new UmbracoExamineIndexConfig(publicAccessService, provider) },
            { Constants.UmbracoIndexes.DeliveryApiContentIndexName, new UmbracoExamineIndexConfig(publicAccessService, provider) }
        };
    }

    public IUmbracoIndexesConfiguration GetConfiguration()
    {
        return new UmbracoIndexesConfiguration(configurationObjects);
    }
}
