using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache;

/// <inheritdoc />
public class CacheManager : ICacheManager
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheManager"/> class.
    /// </summary>
    /// <param name="content">The published content cache.</param>
    /// <param name="media">The published media cache.</param>
    /// <param name="members">The published member cache.</param>
    /// <param name="elements">The published element cache.</param>
    /// <param name="domains">The domain cache.</param>
    /// <param name="elementsCache">The elements-level property value cache.</param>
    public CacheManager(IPublishedContentCache content, IPublishedMediaCache media, IPublishedMemberCache members, IPublishedElementCache elements, IDomainCache domains, IElementsCache elementsCache)
    {
        ElementsCache = elementsCache;
        Content = content;
        Media = media;
        Members = members;
        Elements = elements;
        Domains = domains;
    }

    public IPublishedContentCache Content { get; }

    public IPublishedMediaCache Media { get; }

    public IPublishedMemberCache Members { get; }

    public IPublishedElementCache Elements { get; }

    public IDomainCache Domains { get; }

    public IAppCache ElementsCache { get; }
}
