using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache;

/// <inheritdoc />
public class CacheManager : ICacheManager
{
    public CacheManager(IPublishedContentCache content, IPublishedMediaCache media, IPublishedMemberCache members, IDomainCache domains, IElementsCache elementsCache)
    {
        ElementsCache = elementsCache;
        Content = content;
        Media = media;
        Members = members;
        Domains = domains;
    }

    public IPublishedContentCache Content { get; }

    public IPublishedMediaCache Media { get; }

    public IPublishedMemberCache Members { get; }

    public IDomainCache Domains { get; }

    public IAppCache ElementsCache { get; }
}
