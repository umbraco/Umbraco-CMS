using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Default implementation of <see cref="IDraftContentChangeStrategy"/>: indexes draft documents (including trashed
/// content), and draft media/members/elements, regardless of publish state.
/// </summary>
internal sealed class DraftContentChangeStrategy : ContentChangeStrategyBase, IDraftContentChangeStrategy
{
    private readonly IContentIndexingDataCollectionService _contentIndexingDataCollectionService;
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;
    private readonly IMemberService _memberService;
    private readonly IElementService _elementService;
    private readonly IEntityService _entityService;
    private readonly IEventAggregator _eventAggregator;

    /// <inheritdoc />
    protected override bool SupportsTrashedContent => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="DraftContentChangeStrategy"/> class.
    /// </summary>
    /// <param name="contentIndexingDataCollectionService">The service used to collect the index fields for a content item.</param>
    /// <param name="contentService">The service used to retrieve documents and enumerate the document tree, including the recycle bin.</param>
    /// <param name="mediaService">The service used to retrieve media and enumerate the media tree, including the recycle bin.</param>
    /// <param name="memberService">The service used to retrieve members.</param>
    /// <param name="elementService">The service used to retrieve elements.</param>
    /// <param name="entityService">The service used to enumerate elements, including those beneath element containers and in the recycle bin.</param>
    /// <param name="eventAggregator">The event aggregator used to publish the cancelable content indexing notification.</param>
    /// <param name="umbracoDatabaseFactory">The database factory passed to the base class for paged descendant enumeration.</param>
    /// <param name="idKeyMap">The map passed to the base class for resolving root item keys.</param>
    /// <param name="logger">The logger passed to the base class for logging warnings and rebuild cancellations.</param>
    public DraftContentChangeStrategy(
        IContentIndexingDataCollectionService contentIndexingDataCollectionService,
        IContentService contentService,
        IMediaService mediaService,
        IMemberService memberService,
        IElementService elementService,
        IEntityService entityService,
        IEventAggregator eventAggregator,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IIdKeyMap idKeyMap,
        ILogger<DraftContentChangeStrategy> logger)
        : base(umbracoDatabaseFactory, idKeyMap, logger)
    {
        _contentIndexingDataCollectionService = contentIndexingDataCollectionService;
        _contentService = contentService;
        _mediaService = mediaService;
        _memberService = memberService;
        _elementService = elementService;
        _entityService = entityService;
        _eventAggregator = eventAggregator;
    }

    /// <inheritdoc />
    public async Task HandleAsync(IEnumerable<ContentIndexInfo> indexInfos, IEnumerable<ContentChange> changes, CancellationToken cancellationToken)
    {
        ContentIndexInfo[] indexInfosAsArray = indexInfos as ContentIndexInfo[] ?? indexInfos.ToArray();

        // get the relevant changes for this change strategy
        ContentChange[] changesAsArray = changes.Where(change =>
                change.ContentState is ContentState.Draft
                && change.ObjectType is UmbracoObjectTypes.Document or UmbracoObjectTypes.Media or UmbracoObjectTypes.Member or UmbracoObjectTypes.Element)
            .ToArray();

        var pendingRemovals = new List<ContentChange>();
        foreach (ContentChange change in changesAsArray.Where(change => change.ContentState is ContentState.Draft))
        {
            if (change.ChangeImpact is ChangeImpact.Remove)
            {
                pendingRemovals.Add(change);
            }
            else
            {
                IContentBase? content = GetContent(change);
                if (content is null)
                {
                    // elements are always leaves, so a branch change for an element targets the element container it lives in
                    if (change is { ObjectType: UmbracoObjectTypes.Element, ChangeImpact: ChangeImpact.RefreshWithDescendants })
                    {
                        await RemoveFromIndexAsync(indexInfosAsArray, pendingRemovals);
                        pendingRemovals.Clear();

                        await UpdateIndexForElementContainerDescendantsAsync(indexInfosAsArray, change.Id, cancellationToken);
                        continue;
                    }

                    pendingRemovals.Add(change);
                    continue;
                }

                await RemoveFromIndexAsync(indexInfosAsArray, pendingRemovals);
                pendingRemovals.Clear();

                var updated = await UpdateIndexAsync(indexInfosAsArray, change, content, cancellationToken);
                if (updated is false)
                {
                    pendingRemovals.Add(change);
                }
            }
        }

        await RemoveFromIndexAsync(indexInfosAsArray, pendingRemovals);
    }

    /// <inheritdoc />
    public async Task RebuildAsync(ContentIndexInfo indexInfo, CancellationToken cancellationToken)
    {
        await indexInfo.Indexer.ResetAsync(indexInfo.IndexAlias);

        await RebuildAsync(
            indexInfo,
            UmbracoObjectTypes.Document,
            () => _contentService.GetRootContent(),
            (pageIndex, pageSize) => _contentService.GetPagedChildren(Cms.Core.Constants.System.RecycleBinContent, pageIndex, pageSize, out _),
            cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            LogIndexRebuildCancellation(indexInfo);
            return;
        }

        await RebuildAsync(
            indexInfo,
            UmbracoObjectTypes.Media,
            () => _mediaService.GetRootMedia(),
            (pageIndex, pageSize) => _mediaService.GetPagedChildren(Cms.Core.Constants.System.RecycleBinMedia, pageIndex, pageSize, out _),
            cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            LogIndexRebuildCancellation(indexInfo);
            return;
        }

        await RebuildElementsAsync(indexInfo, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            LogIndexRebuildCancellation(indexInfo);
            return;
        }

        if (indexInfo.ContainedObjectTypes.Contains(UmbracoObjectTypes.Member) is false)
        {
            return;
        }

        IMember[] members;
        var pageIndex = 0;
        do
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            members = _memberService.GetAll(pageIndex, ContentEnumerationPageSize, out _).ToArray();
            foreach (IMember member in members)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                await UpdateIndexAsync([indexInfo], ContentChange.Member(member.Key, ChangeImpact.Refresh, ContentState.Draft), member, cancellationToken);
            }
            pageIndex++;
        }
        while (members.Length == ContentEnumerationPageSize);

        if (cancellationToken.IsCancellationRequested)
        {
            LogIndexRebuildCancellation(indexInfo);
        }
    }

    private async Task<bool> UpdateIndexAsync(ContentIndexInfo[] indexInfos, ContentChange change, IContentBase content, CancellationToken cancellationToken)
    {
        ContentIndexInfo[] applicableIndexInfos = indexInfos.Where(info => info.ContainedObjectTypes.Contains(change.ObjectType)).ToArray();
        if(applicableIndexInfos.Length is 0)
        {
            return true;
        }

        var result = await UpdateIndexAsync(applicableIndexInfos, content, change.ObjectType, cancellationToken);

        if (change.ChangeImpact is ChangeImpact.RefreshWithDescendants)
        {
            switch (change.ObjectType)
            {
                case UmbracoObjectTypes.Document:
                    await EnumerateDescendantsByPath<IContent>(
                        change.ObjectType,
                        content.Key,
                        (id, pageIndex, pageSize, query, ordering) => _contentService
                            .GetPagedDescendants(id, pageIndex, pageSize, out _, query, ordering)
                            .ToArray(),
                        async descendants =>
                            await UpdateIndexDescendantsAsync(applicableIndexInfos, descendants, change.ObjectType, cancellationToken));
                    break;
                case UmbracoObjectTypes.Media:
                    await EnumerateDescendantsByPath<IMedia>(
                        change.ObjectType,
                        content.Key,
                        (id, pageIndex, pageSize, query, ordering) => _mediaService
                            .GetPagedDescendants(id, pageIndex, pageSize, out _, query, ordering)
                            .ToArray(),
                        async descendants =>
                            await UpdateIndexDescendantsAsync(applicableIndexInfos, descendants, change.ObjectType, cancellationToken));
                    break;
            }
        }

        return result;
    }

    private async Task UpdateIndexDescendantsAsync<T>(ContentIndexInfo[] indexInfos, T[] descendants, UmbracoObjectTypes objectType, CancellationToken cancellationToken)
        where T : IContentBase
    {
        foreach (T descendant in descendants)
        {
            await UpdateIndexAsync(indexInfos, descendant, objectType, cancellationToken);
        }
    }

    private async Task<bool> UpdateIndexAsync(ContentIndexInfo[] indexInfos, IContentBase content, UmbracoObjectTypes objectType, CancellationToken cancellationToken)
    {
        IndexField[]? fields = (await _contentIndexingDataCollectionService.CollectAsync(content, false, cancellationToken))?.ToArray();
        if (fields is null)
        {
            return false;
        }

        string?[] cultures = content.AvailableCultures();

        Variation[] variations = content.ContentType.VariesBySegment()
            ? cultures
                .SelectMany(culture => content
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
            var notification = new ContentIndexingNotification(indexInfo.IndexAlias, content.Key, UmbracoObjectTypes.Document, variations, fields);
            if (await _eventAggregator.PublishCancelableAsync(notification))
            {
                // the indexing operation was cancelled for this index; continue with the rest of the indexes
                continue;
            }

            await indexInfo.Indexer.AddOrUpdateAsync(indexInfo.IndexAlias, content.Key, objectType, variations, notification.Fields, null);
        }

        return true;
    }

    private async Task RemoveFromIndexAsync(ContentIndexInfo[] indexInfos, IReadOnlyCollection<ContentChange> contentChanges)
    {
        if (contentChanges.Count is 0)
        {
            return;
        }

        foreach (ContentIndexInfo indexInfo in indexInfos)
        {
            Guid[] keys = contentChanges
                .Where(change => indexInfo.ContainedObjectTypes.Contains(change.ObjectType))
                .Select(change => change.Id)
                .ToArray();
            await indexInfo.Indexer.DeleteAsync(indexInfo.IndexAlias, keys);
        }
    }

    private async Task UpdateIndexForElementContainerDescendantsAsync(ContentIndexInfo[] indexInfos, Guid containerKey, CancellationToken cancellationToken)
    {
        ContentIndexInfo[] applicableIndexInfos = indexInfos.Where(info => info.ContainedObjectTypes.Contains(UmbracoObjectTypes.Element)).ToArray();
        if (applicableIndexInfos.Length is 0)
        {
            return;
        }

        var skip = 0;
        IEntitySlim[] descendants;
        do
        {
            descendants = _entityService
                .GetPagedDescendants(containerKey, UmbracoObjectTypes.ElementContainer, [UmbracoObjectTypes.Element], skip, ContentEnumerationPageSize, out _, ordering: Ordering.By("Path"))
                .ToArray();

            await UpdateIndexForElementsAsync(applicableIndexInfos, descendants, cancellationToken);
            skip += ContentEnumerationPageSize;
        }
        while (descendants.Length == ContentEnumerationPageSize && cancellationToken.IsCancellationRequested is false);
    }

    private async Task RebuildElementsAsync(ContentIndexInfo indexInfo, CancellationToken cancellationToken)
    {
        if (indexInfo.ContainedObjectTypes.Contains(UmbracoObjectTypes.Element) is false)
        {
            return;
        }

        var pageIndex = 0;
        IEntitySlim[] elements;
        do
        {
            elements = _entityService
                .GetPagedDescendants(UmbracoObjectTypes.Element, pageIndex, ContentEnumerationPageSize, out _, ordering: Ordering.By("Path"), includeTrashed: true)
                .ToArray();

            await UpdateIndexForElementsAsync([indexInfo], elements, cancellationToken);
            pageIndex++;
        }
        while (elements.Length == ContentEnumerationPageSize && cancellationToken.IsCancellationRequested is false);
    }

    private async Task UpdateIndexForElementsAsync(ContentIndexInfo[] indexInfos, IEntitySlim[] elementEntities, CancellationToken cancellationToken)
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

            await UpdateIndexAsync(indexInfos, element, UmbracoObjectTypes.Element, cancellationToken);
        }
    }

    private IContentBase? GetContent(ContentChange change)
        => change.ObjectType switch
        {
            UmbracoObjectTypes.Document => _contentService.GetById(change.Id),
            UmbracoObjectTypes.Media => _mediaService.GetById(change.Id),
            UmbracoObjectTypes.Member => _memberService.GetById(change.Id),
            UmbracoObjectTypes.Element => _elementService.GetById(change.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(change), change.ObjectType, "This strategy only supports documents, media, members and elements")
        };

    private async Task RebuildAsync(
        ContentIndexInfo indexInfo,
        UmbracoObjectTypes objectType,
        Func<IEnumerable<IContentBase>> getContentAtRoot,
        Func<int, int, IEnumerable<IContentBase>> getPagedContentAtRecycleBinRoot,
        CancellationToken cancellationToken)
    {
        if (indexInfo.ContainedObjectTypes.Contains(objectType) is false)
        {
            return;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            LogIndexRebuildCancellation(indexInfo);
            return;
        }

        ContentIndexInfo[] indexInfos = [indexInfo];

        foreach (IContentBase rootContent in getContentAtRoot())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await UpdateIndexAsync(indexInfos, GetContentChange(rootContent), rootContent, cancellationToken);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            LogIndexRebuildCancellation(indexInfo);
            return;
        }

        IContentBase[] contentInRecycleBin;
        var pageIndex = 0;
        do
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            contentInRecycleBin = getPagedContentAtRecycleBinRoot(pageIndex, ContentEnumerationPageSize).ToArray();
            foreach (IContentBase content in contentInRecycleBin)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                await UpdateIndexAsync(indexInfos, GetContentChange(content), content, cancellationToken);
            }
            pageIndex++;
        }
        while (contentInRecycleBin.Length == ContentEnumerationPageSize);

        return;

        ContentChange GetContentChange(IContentBase content)
        {
            ContentChange contentChange = objectType switch
            {
                UmbracoObjectTypes.Document => ContentChange.Document(content.Key, ChangeImpact.RefreshWithDescendants, ContentState.Draft),
                UmbracoObjectTypes.Media => ContentChange.Media(content.Key, ChangeImpact.RefreshWithDescendants, ContentState.Draft),
                UmbracoObjectTypes.Member => ContentChange.Member(content.Key, ChangeImpact.Refresh, ContentState.Draft),
                _ => throw new ArgumentOutOfRangeException(nameof(content), objectType, "This strategy only supports documents, media and members")
            };
            return contentChange;
        }
    }
}
