using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Default implementation of <see cref="IDraftElementChangeStrategy"/>: indexes draft library elements (including trashed
/// elements), regardless of publish state.
/// </summary>
/// <remarks>
/// Library elements are always leaves, organised in element containers. A change that affects descendants
/// (<see cref="ChangeImpact.RefreshWithDescendants"/>) therefore targets an element container, and re-indexes every
/// element beneath it.
/// </remarks>
internal sealed class DraftElementChangeStrategy : IDraftElementChangeStrategy
{
    private const int PageSize = ContentChangeStrategyBase.ContentEnumerationPageSize;

    private readonly IContentIndexingDataCollectionService _contentIndexingDataCollectionService;
    private readonly IElementService _elementService;
    private readonly IEntityService _entityService;
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger<DraftElementChangeStrategy> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DraftElementChangeStrategy"/> class.
    /// </summary>
    /// <param name="contentIndexingDataCollectionService">The service used to collect the index fields for an element.</param>
    /// <param name="elementService">The service used to retrieve elements.</param>
    /// <param name="entityService">The service used to enumerate elements, including those beneath element containers and in the recycle bin.</param>
    /// <param name="eventAggregator">The event aggregator used to publish the cancelable content indexing notification.</param>
    /// <param name="logger">The logger used to log rebuild cancellations.</param>
    public DraftElementChangeStrategy(
        IContentIndexingDataCollectionService contentIndexingDataCollectionService,
        IElementService elementService,
        IEntityService entityService,
        IEventAggregator eventAggregator,
        ILogger<DraftElementChangeStrategy> logger)
    {
        _contentIndexingDataCollectionService = contentIndexingDataCollectionService;
        _elementService = elementService;
        _entityService = entityService;
        _eventAggregator = eventAggregator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(IEnumerable<ContentIndexInfo> indexInfos, IEnumerable<ContentChange> changes, CancellationToken cancellationToken)
    {
        ContentIndexInfo[] applicableIndexInfos = indexInfos
            .Where(info => info.ContainedObjectTypes.Contains(UmbracoObjectTypes.Element))
            .ToArray();
        if (applicableIndexInfos.Length is 0)
        {
            return;
        }

        ContentChange[] elementChanges = changes
            .Where(change => change is { ContentState: ContentState.Draft, ObjectType: UmbracoObjectTypes.Element })
            .ToArray();

        var pendingRemovals = new List<Guid>();
        foreach (ContentChange change in elementChanges)
        {
            if (change.ChangeImpact is ChangeImpact.Remove)
            {
                pendingRemovals.Add(change.Id);
                continue;
            }

            IElement? element = _elementService.GetById(change.Id);
            if (element is not null)
            {
                await RemoveFromIndexAsync(applicableIndexInfos, pendingRemovals);
                pendingRemovals.Clear();

                if (await UpdateIndexAsync(applicableIndexInfos, element, cancellationToken) is false)
                {
                    pendingRemovals.Add(change.Id);
                }

                continue;
            }

            if (change.ChangeImpact is ChangeImpact.RefreshWithDescendants)
            {
                await RemoveFromIndexAsync(applicableIndexInfos, pendingRemovals);
                pendingRemovals.Clear();

                await UpdateIndexForContainerDescendantsAsync(applicableIndexInfos, change.Id, cancellationToken);
                continue;
            }

            pendingRemovals.Add(change.Id);
        }

        await RemoveFromIndexAsync(applicableIndexInfos, pendingRemovals);
    }

    /// <inheritdoc />
    public async Task RebuildAsync(ContentIndexInfo indexInfo, CancellationToken cancellationToken)
    {
        await indexInfo.Indexer.ResetAsync(indexInfo.IndexAlias);

        if (indexInfo.ContainedObjectTypes.Contains(UmbracoObjectTypes.Element) is false)
        {
            return;
        }

        var pageIndex = 0;
        IEntitySlim[] elements;
        do
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cancellation requested for rebuild of index: {indexAlias}", indexInfo.IndexAlias);
                return;
            }

            elements = _entityService
                .GetPagedDescendants(UmbracoObjectTypes.Element, pageIndex, PageSize, out _, ordering: Ordering.By("Path"), includeTrashed: true)
                .ToArray();

            await UpdateIndexAsync([indexInfo], elements, cancellationToken);
            pageIndex++;
        }
        while (elements.Length == PageSize);
    }

    private async Task UpdateIndexForContainerDescendantsAsync(ContentIndexInfo[] indexInfos, Guid containerKey, CancellationToken cancellationToken)
    {
        var skip = 0;
        IEntitySlim[] descendants;
        do
        {
            descendants = _entityService
                .GetPagedDescendants(containerKey, UmbracoObjectTypes.ElementContainer, [UmbracoObjectTypes.Element], skip, PageSize, out _, ordering: Ordering.By("Path"))
                .ToArray();

            await UpdateIndexAsync(indexInfos, descendants, cancellationToken);
            skip += PageSize;
        }
        while (descendants.Length == PageSize && cancellationToken.IsCancellationRequested is false);
    }

    private async Task UpdateIndexAsync(ContentIndexInfo[] indexInfos, IEntitySlim[] elementEntities, CancellationToken cancellationToken)
    {
        if (elementEntities.Length is 0)
        {
            return;
        }

        foreach (IElement element in _elementService.GetByIds(elementEntities.Select(entity => entity.Key)))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await UpdateIndexAsync(indexInfos, element, cancellationToken);
        }
    }

    private async Task<bool> UpdateIndexAsync(ContentIndexInfo[] indexInfos, IElement element, CancellationToken cancellationToken)
    {
        IndexField[]? fields = (await _contentIndexingDataCollectionService.CollectAsync(element, false, cancellationToken))?.ToArray();
        if (fields is null)
        {
            return false;
        }

        string?[] cultures = element.AvailableCultures();

        Variation[] variations = element.ContentType.VariesBySegment()
            ? cultures
                .SelectMany(culture => element
                    .Properties
                    .SelectMany(property => property.Values.Where(value => value.Culture.InvariantEquals(culture)))
                    .DistinctBy(value => value.Segment).Select(value => value.Segment)
                    .Select(segment => new Variation(culture, segment)))
                .ToArray()
            : cultures
                .Select(culture => new Variation(culture, null))
                .ToArray();

        foreach (ContentIndexInfo indexInfo in indexInfos)
        {
            var notification = new ContentIndexingNotification(indexInfo.IndexAlias, element.Key, UmbracoObjectTypes.Element, variations, fields);
            if (await _eventAggregator.PublishCancelableAsync(notification))
            {
                // the indexing operation was cancelled for this index; continue with the rest of the indexes
                continue;
            }

            await indexInfo.Indexer.AddOrUpdateAsync(indexInfo.IndexAlias, element.Key, UmbracoObjectTypes.Element, variations, notification.Fields, null);
        }

        return true;
    }

    private static async Task RemoveFromIndexAsync(ContentIndexInfo[] indexInfos, IReadOnlyCollection<Guid> keys)
    {
        if (keys.Count is 0)
        {
            return;
        }

        foreach (ContentIndexInfo indexInfo in indexInfos)
        {
            await indexInfo.Indexer.DeleteAsync(indexInfo.IndexAlias, keys.ToArray());
        }
    }
}
