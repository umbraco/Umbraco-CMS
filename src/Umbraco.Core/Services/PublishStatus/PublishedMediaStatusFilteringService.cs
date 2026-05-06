using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Filters published media based on availability.
/// </summary>
/// <remarks>
/// NOTE: this class is basically a no-op implementation of IPublishStatusQueryService, because the published
/// content extensions need a media equivalent to the content implementation.
/// Incidentally, if we'll ever support variant and/or draft media, this comes in really handy :-)
/// </remarks>
internal sealed class PublishedMediaStatusFilteringService : IPublishedMediaStatusFilteringService
{
    private readonly IPublishedMediaCache _publishedMediaCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedMediaStatusFilteringService"/> class.
    /// </summary>
    /// <param name="publishedMediaCache">The published media cache for retrieving media items.</param>
    public PublishedMediaStatusFilteringService(IPublishedMediaCache publishedMediaCache)
        => _publishedMediaCache = publishedMediaCache;

    /// <inheritdoc />
    public IEnumerable<IPublishedContent> FilterAvailable(IEnumerable<Guid> candidateKeys, string? culture)
        => candidateKeys.Select(_publishedMediaCache.GetById).WhereNotNull().ToArray();
}
