using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

internal sealed class PublishedContentChangeStrategy : ContentChangeStrategyBase, IPublishedContentChangeStrategy
{
    private readonly IContentIndexingDataCollectionService _contentIndexingDataCollectionService;
    private readonly IContentProtectionProvider _contentProtectionProvider;
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;
    private readonly IMemberService _memberService;
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger<PublishedContentChangeStrategy> _logger;

    protected override bool SupportsTrashedContent => false;

    public PublishedContentChangeStrategy(
        IContentIndexingDataCollectionService contentIndexingDataCollectionService,
        IContentProtectionProvider contentProtectionProvider,
        IContentService contentService,
        IMediaService mediaService,
        IMemberService memberService,
        IEventAggregator eventAggregator,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IIdKeyMap idKeyMap,
        ILogger<PublishedContentChangeStrategy> logger)
        : base(umbracoDatabaseFactory, idKeyMap, logger)
    {
        _contentIndexingDataCollectionService = contentIndexingDataCollectionService;
        _contentProtectionProvider = contentProtectionProvider;
        _contentService = contentService;
        _mediaService = mediaService;
        _memberService = memberService;
        _logger = logger;
        _eventAggregator = eventAggregator;
    }

    public async Task HandleAsync(IEnumerable<ContentIndexInfo> indexInfos, IEnumerable<ContentChange> changes, CancellationToken cancellationToken)
    {
        // make sure all indexes can handle documents, media or members
        ContentIndexInfo[] indexInfosAsArray = indexInfos as ContentIndexInfo[] ?? indexInfos.ToArray();
        if (indexInfosAsArray.Any(indexInfo => ContainsSupportedObjectType(indexInfo) is false))
        {
            _logger.LogWarning("One or more indexes for unsupported object types were detected and skipped. This strategy only supports Documents, Media and Members.");
            indexInfosAsArray = indexInfosAsArray.Where(ContainsSupportedObjectType).ToArray();
        }

        // get the relevant changes for this change strategy:
        // - published content
        // - media and members in any state (implicitly always draft)
        ContentChange[] changesAsArray = changes.Where(change =>
                change is { ObjectType: UmbracoObjectTypes.Document, ContentState: ContentState.Published }
                || change.ObjectType is UmbracoObjectTypes.Media or UmbracoObjectTypes.Member)
            .ToArray();

        var pendingRemovals = new List<ContentChange>();
        foreach (ContentChange change in changesAsArray)
        {
            if (change.ChangeImpact is ChangeImpact.Remove)
            {
                pendingRemovals.Add(change);
            }
            else
            {
                IContentBase? content = GetContent(change);
                if (content is null || content.Trashed)
                {
                    pendingRemovals.Add(change);
                    continue;
                }

                await RemoveFromIndexAsync(indexInfosAsArray, pendingRemovals);
                pendingRemovals.Clear();

                ContentIndexInfo[] applicableIndexInfos = indexInfosAsArray
                    .Where(info => info.ContainedObjectTypes.Contains(change.ObjectType))
                    .ToArray();
                await ReindexAsync(applicableIndexInfos, content, change.ObjectType, change.ChangeImpact is ChangeImpact.RefreshWithDescendants, cancellationToken);
            }
        }

        await RemoveFromIndexAsync(indexInfosAsArray, pendingRemovals);
    }

    public async Task RebuildAsync(ContentIndexInfo indexInfo, CancellationToken cancellationToken)
    {
        await indexInfo.Indexer.ResetAsync(indexInfo.IndexAlias);

        ContentIndexInfo[] indexInfos = [indexInfo];

        if (indexInfo.ContainedObjectTypes.Contains(UmbracoObjectTypes.Document))
        {
            foreach (IContent content in _contentService.GetRootContent())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    LogIndexRebuildCancellation(indexInfo);
                    return;
                }

                await ReindexAsync(indexInfos, content, UmbracoObjectTypes.Document, true, cancellationToken);
            }
        }

        if (indexInfo.ContainedObjectTypes.Contains(UmbracoObjectTypes.Media))
        {
            foreach (IMedia media in _mediaService.GetRootMedia())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    LogIndexRebuildCancellation(indexInfo);
                    return;
                }

                await ReindexAsync(indexInfos, media, UmbracoObjectTypes.Media, true, cancellationToken);
            }
        }

        if (indexInfo.ContainedObjectTypes.Contains(UmbracoObjectTypes.Member))
        {
            var pageIndex = 0;
            long totalRecords;
            do
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    LogIndexRebuildCancellation(indexInfo);
                    return;
                }

                IMember[] members = _memberService.GetAll(pageIndex, ContentEnumerationPageSize, out totalRecords).ToArray();
                foreach (IMember member in members)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        LogIndexRebuildCancellation(indexInfo);
                        return;
                    }

                    await ReindexAsync(indexInfos, member, UmbracoObjectTypes.Member, false, cancellationToken);
                }

                pageIndex++;
            }
            while (pageIndex * ContentEnumerationPageSize < totalRecords);
        }
    }

    private async Task ReindexAsync(ContentIndexInfo[] indexInfos, IContentBase content, UmbracoObjectTypes objectType, bool forceReindexDescendants, CancellationToken cancellationToken)
    {
        // index the content
        Variation[] indexedVariants = await UpdateIndexAsync(indexInfos, content, objectType, cancellationToken);
        if (indexedVariants.Any() is false)
        {
            // we likely got here because a removal triggered a "refresh branch" notification, now we
            // need to delete every last culture of this content and all descendants
            await RemoveFromIndexAsync(indexInfos, content.Key);
            return;
        }

        if (forceReindexDescendants)
        {
            await ReindexDescendantsAsync(indexInfos, content, objectType, cancellationToken);
        }
    }

    private async Task ReindexDescendantsAsync(ContentIndexInfo[] indexInfos, IContentBase content, UmbracoObjectTypes objectType, CancellationToken cancellationToken)
    {
        var removedDescendantIds = new List<int>();

        async Task ProcessDescendants<T>(T[] descendants) where T : IContentBase
        {
            // NOTE: this works because we're enumerating descendants by path
            foreach (T descendant in descendants)
            {
                if (removedDescendantIds.Contains(descendant.ParentId))
                {
                    continue;
                }

                Variation[] indexedVariants = await UpdateIndexAsync(indexInfos, descendant, objectType, cancellationToken);
                if (indexedVariants.Any() is false)
                {
                    // no variants to index, make sure this is removed from the index and skip any descendants moving forward
                    // (the index implementation is responsible for deleting descendants at index level)
                    await RemoveFromIndexAsync(indexInfos, descendant.Key);
                    removedDescendantIds.Add(descendant.Id);
                }
            }
        }

        switch (objectType)
        {
            case UmbracoObjectTypes.Document:
                await EnumerateDescendantsByPath<IContent>(
                    objectType,
                    content.Key,
                    (id, pageIndex, pageSize, query, ordering) => _contentService
                        .GetPagedDescendants(id, pageIndex, pageSize, out _, query, ordering)
                        .ToArray(),
                    ProcessDescendants);
                break;
            case UmbracoObjectTypes.Media:
                await EnumerateDescendantsByPath<IMedia>(
                    objectType,
                    content.Key,
                    (id, pageIndex, pageSize, query, ordering) => _mediaService
                        .GetPagedDescendants(id, pageIndex, pageSize, out _, query, ordering)
                        .ToArray(),
                    ProcessDescendants);
                break;
        }
    }

    private async Task<Variation[]> UpdateIndexAsync(ContentIndexInfo[] indexInfos, IContentBase content, UmbracoObjectTypes objectType, CancellationToken cancellationToken)
    {
        Variation[] variations = objectType is UmbracoObjectTypes.Document
            ? RoutablePublishedVariations(content)
            : NonDocumentVariations(content);
        if (variations.Length is 0)
        {
            return [];
        }

        IEnumerable<IndexField>? fields = await _contentIndexingDataCollectionService.CollectAsync(content, true, cancellationToken);
        if (fields is null)
        {
            return [];
        }

        // the fields collection is for all published variants of the content - but it's not certain that a published
        // variant is also routable, because the published routing state can be broken at ancestor level.
        fields = fields.Where(field => variations.Any(v => (field.Culture is null || v.Culture == field.Culture) && (field.Segment is null || v.Segment == field.Segment))).ToArray();

        ContentProtection? contentProtection = objectType is UmbracoObjectTypes.Document
            ? await _contentProtectionProvider.GetContentProtectionAsync(content)
            : null;

        foreach (ContentIndexInfo indexInfo in indexInfos)
        {
            var notification = new ContentIndexingNotification(indexInfo.IndexAlias, content.Key, objectType, variations, fields);
            if (await _eventAggregator.PublishCancelableAsync(notification))
            {
                // the indexing operation was cancelled for this index; continue with the rest of the indexes
                continue;
            }

            await indexInfo.Indexer.AddOrUpdateAsync(indexInfo.IndexAlias, content.Key, objectType, variations, notification.Fields, contentProtection);
        }

        return variations;
    }

    private async Task RemoveFromIndexAsync(ContentIndexInfo[] indexInfos, Guid id)
        => await RemoveFromIndexAsync(indexInfos, [id]);

    private async Task RemoveFromIndexAsync(ContentIndexInfo[] indexInfos, IReadOnlyCollection<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return;
        }

        foreach (ContentIndexInfo indexInfo in indexInfos)
        {
            await indexInfo.Indexer.DeleteAsync(indexInfo.IndexAlias, ids);
        }
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
            if (keys.Length > 0)
            {
                await indexInfo.Indexer.DeleteAsync(indexInfo.IndexAlias, keys);
            }
        }
    }

    private IContentBase? GetContent(ContentChange change)
        => change.ObjectType switch
        {
            UmbracoObjectTypes.Document => _contentService.GetById(change.Id),
            UmbracoObjectTypes.Media => _mediaService.GetById(change.Id),
            UmbracoObjectTypes.Member => _memberService.GetById(change.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(change.ObjectType))
        };

    private static bool ContainsSupportedObjectType(ContentIndexInfo indexInfo)
        => indexInfo.ContainedObjectTypes.Contains(UmbracoObjectTypes.Document)
           || indexInfo.ContainedObjectTypes.Contains(UmbracoObjectTypes.Media)
           || indexInfo.ContainedObjectTypes.Contains(UmbracoObjectTypes.Member);

    // NOTE: for the time being, segments are not individually publishable, but it will likely happen at some point,
    //       so this method deals with variations - not cultures.
    private Variation[] RoutablePublishedVariations(IContentBase content)
    {
        if (content.IsPublished() is false)
        {
            return [];
        }

        var variesByCulture = content.VariesByCulture();

        // if the content varies by culture, the indexable cultures are the published
        // cultures - otherwise "null" represents "no culture"
        var cultures = content.PublishedCultures();

        // now iterate all ancestors and make sure all cultures are published all the way up the tree
        foreach (var ancestorId in content.AncestorIds())
        {
            IContent? ancestor = _contentService.GetById(ancestorId);
            if (ancestor is null || ancestor.Published is false)
            {
                // no published ancestor => don't index anything
                cultures = [];
            }
            else if (variesByCulture && ancestor.VariesByCulture())
            {
                // both the content and the ancestor are culture variant => only index the published cultures they have in common
                cultures = cultures.Intersect(ancestor.PublishedCultures).ToArray();
            }

            // if we've already run out of cultures to index, there is no reason to iterate the ancestors any further
            if (cultures.Any() == false)
            {
                break;
            }
        }

        // for now, segments are not individually routable, so we only need to deal with cultures and append all known segments
        if (content.Properties.Any(p => p.PropertyType.VariesBySegment()) is false)
        {
            // no segment variant properties - just return the found cultures
            return cultures.Select(c => new Variation(c, null)).ToArray();
        }

        // segments are not "known" - we can only determine segment variation by looking at the property values
        return cultures.SelectMany(culture => content
                .Properties
                .SelectMany(property => property.Values.Where(value => value.Culture.InvariantEquals(culture)))
                .DistinctBy(value => value.Segment).Select(value => value.Segment)
                .Select(segment => new Variation(culture, segment)))
            .ToArray();
    }

    private Variation[] NonDocumentVariations(IContentBase content)
    {
        string?[] cultures = content.AvailableCultures();

        if (content.Properties.Any(p => p.PropertyType.VariesBySegment()) is false)
        {
            return cultures.Select(c => new Variation(c, null)).ToArray();
        }

        return cultures.SelectMany(culture => content
                .Properties
                .SelectMany(property => property.Values.Where(value => value.Culture.InvariantEquals(culture)))
                .DistinctBy(value => value.Segment).Select(value => value.Segment)
                .Select(segment => new Variation(culture, segment)))
            .ToArray();
    }
}
