using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Navigation;

// NOTE: this class is basically a no-op implementation of IPublishStatusQueryService, because the published
//       content extensions need a media equivalent to the content implementation.
//       incidentally, if we'll ever support variant and/or draft media, this comes in really handy :-)
internal sealed class PublishedMediaStatusFilteringService : IPublishedMediaStatusFilteringService
{
    private readonly IPublishedMediaCache _publishedMediaCache;

    public PublishedMediaStatusFilteringService(IPublishedMediaCache publishedMediaCache)
        => _publishedMediaCache = publishedMediaCache;

    public IEnumerable<IPublishedContent> FilterAncestors(IEnumerable<Guid> ancestorsKeys, string? culture)
        => GetAll(ancestorsKeys);

    public IEnumerable<IPublishedContent> FilterSiblings(IEnumerable<Guid> siblingKeys, string? culture)
        => GetAll(siblingKeys);

    public IEnumerable<IPublishedContent> FilterChildren(IEnumerable<Guid> childrenKeys, string? culture)
        => GetAll(childrenKeys);

    public IEnumerable<IPublishedContent> FilterDescendants(IEnumerable<Guid> descendantKeys, string? culture)
        => GetAll(descendantKeys);

    private IEnumerable<IPublishedContent> GetAll(IEnumerable<Guid> keys) => keys.Select(_publishedMediaCache.GetById).WhereNotNull().ToArray();
}
