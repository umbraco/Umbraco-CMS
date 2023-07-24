using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Search.Configuration;

namespace Umbraco.Search.InMemory.Configuration;

public class InMemoryIndexConfigurationFactory : IIndexConfigurationFactory
{
    private readonly Dictionary<string, IUmbracoIndexConfiguration> configurationObjects;

    public InMemoryIndexConfigurationFactory(IPublicAccessService publicAccessService, IScopeProvider provider)
    {
        configurationObjects = new Dictionary<string, IUmbracoIndexConfiguration>()
        {
            { Constants.UmbracoIndexes.InternalIndexName, new UmbracoInMemoryIndexConfig() },
            { Constants.UmbracoIndexes.ExternalIndexName, new UmbracoInMemoryIndexConfig(true) },
            { Constants.UmbracoIndexes.MembersIndexName, new UmbracoInMemoryIndexConfig() },
            { Constants.UmbracoIndexes.DeliveryApiContentIndexName, new UmbracoInMemoryIndexConfig()}
        };
    }

    public IUmbracoIndexesConfiguration GetConfiguration()
    {
        return new UmbracoIndexesConfiguration(configurationObjects);
    }
}
