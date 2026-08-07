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
    private readonly IMediaCacheService _mediaCacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedMediaStatusFilteringService"/> class.
    /// </summary>
    /// <param name="publishedMediaCache">The published media cache for retrieving media items.</param>
    /// <param name="mediaCacheService">The media cache service used to materialise candidate keys in batches.</param>
    public PublishedMediaStatusFilteringService(IPublishedMediaCache publishedMediaCache, IMediaCacheService mediaCacheService)
    {
        _publishedMediaCache = publishedMediaCache;
        _mediaCacheService = mediaCacheService;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Materialised in growing chunks: an all-L0-hit chunk stays fully synchronous, while a cold set
    /// collapses its database access into batched reads. Returned lazily so consumers like
    /// .FirstOrDefault() / .Take(n) can short-circuit without materialising the full result. Callers
    /// that need to enumerate the result more than once should buffer it themselves (.ToList() / .ToArray()).
    /// </remarks>
    public IEnumerable<IPublishedContent> FilterAvailable(IEnumerable<Guid> candidateKeys, string? culture)
        => ChunkedPublishedContentEnumerator.Enumerate(
            candidateKeys,
            _mediaCacheService.TryGetCached,
            misses => _mediaCacheService.GetByKeysAsync(misses).GetAwaiter().GetResult(),
            predicate: null);

    /// <inheritdoc />
    public IEnumerable<IPublishedContent> Unfiltered(IEnumerable<Guid> candidateKeys)
        => candidateKeys.Select(_publishedMediaCache.GetById).WhereNotNull();
}
