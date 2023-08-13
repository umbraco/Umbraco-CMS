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
            {
                global::Umbraco.Cms.Core.Constants.UmbracoIndexes.InternalIndexName,
                new UmbracoExamineIndexConfig(publicAccessService, provider)
            },
            {
                global::Umbraco.Cms.Core.Constants.UmbracoIndexes.ExternalIndexName,
                new UmbracoExamineIndexConfig(publicAccessService, provider) { PublishedValuesOnly = true }
            },
            {
                global::Umbraco.Cms.Core.Constants.UmbracoIndexes.MembersIndexName,
                new UmbracoExamineIndexConfig(publicAccessService, provider)
            },
            {
                global::Umbraco.Cms.Core.Constants.UmbracoIndexes.DeliveryApiContentIndexName,
                new UmbracoExamineIndexConfig(publicAccessService, provider)
            }
        };
    }

    public IUmbracoIndexesConfiguration GetConfiguration()
    {
        return new UmbracoIndexesConfiguration(configurationObjects);
    }
}
