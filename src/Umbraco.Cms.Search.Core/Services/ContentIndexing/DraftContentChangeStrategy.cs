using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Helpers;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Models.Persistence;
using Umbraco.Cms.Search.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Default implementation of <see cref="IDraftContentChangeStrategy"/>: indexes draft documents (including trashed
/// content), and draft media/members, regardless of publish state. Members are indexed regardless of whether they
/// are backed by a full content-based <see cref="IMember"/> or a lightweight <see cref="ExternalMemberIdentity"/> -
/// both share the same members index.
/// </summary>
internal sealed class DraftContentChangeStrategy : ContentChangeStrategyBase, IDraftContentChangeStrategy
{
    private readonly IContentIndexingDataCollectionService _contentIndexingDataCollectionService;
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;
    private readonly IMemberService _memberService;
    private readonly IExternalMemberService _externalMemberService;
    private readonly IIndexDocumentService _indexDocumentService;
    private readonly IDateTimeOffsetConverter _dateTimeOffsetConverter;
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
    /// <param name="externalMemberService">The service used to retrieve lightweight external members, tried as a fallback when a member change does not resolve via <paramref name="memberService"/>.</param>
    /// <param name="indexDocumentService">The service used to look up and persist index document snapshots for external members (which bypass <see cref="IContentIndexingDataCollectionService"/>, as they are not <see cref="IContentBase"/>).</param>
    /// <param name="dateTimeOffsetConverter">The converter used to normalize external members' indexed date values to <see cref="DateTimeOffset"/>.</param>
    /// <param name="eventAggregator">The event aggregator used to publish the cancelable content indexing notification.</param>
    /// <param name="umbracoDatabaseFactory">The database factory passed to the base class for paged descendant enumeration.</param>
    /// <param name="idKeyMap">The map passed to the base class for resolving root item keys.</param>
    /// <param name="logger">The logger passed to the base class for logging warnings and rebuild cancellations.</param>
    public DraftContentChangeStrategy(
        IContentIndexingDataCollectionService contentIndexingDataCollectionService,
        IContentService contentService,
        IMediaService mediaService,
        IMemberService memberService,
        IExternalMemberService externalMemberService,
        IIndexDocumentService indexDocumentService,
        IDateTimeOffsetConverter dateTimeOffsetConverter,
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
        _externalMemberService = externalMemberService;
        _indexDocumentService = indexDocumentService;
        _dateTimeOffsetConverter = dateTimeOffsetConverter;
        _eventAggregator = eventAggregator;
    }

    /// <inheritdoc />
    public async Task HandleAsync(IEnumerable<ContentIndexInfo> indexInfos, IEnumerable<ContentChange> changes, CancellationToken cancellationToken)
    {
        ContentIndexInfo[] indexInfosAsArray = indexInfos as ContentIndexInfo[] ?? indexInfos.ToArray();

        // get the relevant changes for this change strategy
        ContentChange[] changesAsArray = changes.Where(change =>
                change.ContentState is ContentState.Draft
                && change.ObjectType is UmbracoObjectTypes.Document or UmbracoObjectTypes.Media or UmbracoObjectTypes.Member)
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
                    // members have two possible backing representations - a full content-based IMember
                    // (handled above) or a lightweight ExternalMemberIdentity - both share this index.
                    ExternalMemberIdentity? externalMember = change.ObjectType is UmbracoObjectTypes.Member
                        ? await _externalMemberService.GetByKeyAsync(change.Id)
                        : null;

                    if (externalMember is null)
                    {
                        pendingRemovals.Add(change);
                        continue;
                    }

                    await RemoveFromIndexAsync(indexInfosAsArray, pendingRemovals);
                    pendingRemovals.Clear();

                    ContentIndexInfo[] applicableIndexInfos = indexInfosAsArray.Where(info => info.ContainedObjectTypes.Contains(UmbracoObjectTypes.Member)).ToArray();
                    await UpdateIndexForExternalMemberAsync(applicableIndexInfos, externalMember, cancellationToken);
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
            return;
        }

        // external members share this index with content-based members (indexed above).
        long externalMembersTotal;
        var externalMembersSkip = 0;
        do
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            PagedModel<ExternalMemberIdentity> page = await _externalMemberService.GetAllAsync(externalMembersSkip, ContentEnumerationPageSize);
            externalMembersTotal = page.Total;
            foreach (ExternalMemberIdentity externalMember in page.Items)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                await UpdateIndexForExternalMemberAsync([indexInfo], externalMember, cancellationToken);
            }
            externalMembersSkip += ContentEnumerationPageSize;
        }
        while (externalMembersSkip < externalMembersTotal);

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

    // external members are not IContentBase (no content type, tree or properties), so they bypass
    // IContentIndexingDataCollectionService/ISystemFieldsContentIndexer entirely and are indexed via
    // this dedicated path instead - reusing IIndexDocumentService directly for change-detection parity
    // with the regular content pipeline.
    private async Task UpdateIndexForExternalMemberAsync(ContentIndexInfo[] indexInfos, ExternalMemberIdentity member, CancellationToken cancellationToken)
    {
        if (indexInfos.Length is 0)
        {
            return;
        }

        IndexField[] fields = await CollectExternalMemberFieldsAsync(member);
        Variation[] variations = [new Variation(null, null)];

        foreach (ContentIndexInfo indexInfo in indexInfos)
        {
            var notification = new ContentIndexingNotification(indexInfo.IndexAlias, member.Key, UmbracoObjectTypes.Member, variations, fields);
            if (await _eventAggregator.PublishCancelableAsync(notification))
            {
                // the indexing operation was cancelled for this index; continue with the rest of the indexes
                continue;
            }

            await indexInfo.Indexer.AddOrUpdateAsync(indexInfo.IndexAlias, member.Key, UmbracoObjectTypes.Member, variations, notification.Fields, null);
        }
    }

    private async Task<IndexField[]> CollectExternalMemberFieldsAsync(ExternalMemberIdentity member)
    {
        IndexDocument? document = await _indexDocumentService.GetAsync(member.Key, published: false);
        if (document is not null)
        {
            return document.Fields;
        }

        IndexField[] fields = BuildExternalMemberIndexFields(member);

        await _indexDocumentService.AddAsync(new IndexDocument
        {
            Key = member.Key,
            Fields = fields,
            Published = false,
        });

        return fields;
    }

    private IndexField[] BuildExternalMemberIndexFields(ExternalMemberIdentity member)
    {
        var name = member.Name.IsNullOrWhiteSpace() is false ? member.Name! : member.UserName;

        return
        [
            new IndexField(Constants.FieldNames.Id, new IndexValue { Keywords = [member.Key.AsKeyword()] }, null, null),
            new IndexField(Constants.FieldNames.ObjectType, new IndexValue { Keywords = [UmbracoObjectTypes.Member.ToString()] }, null, null),
            new IndexField(Constants.FieldNames.CreateDate, new IndexValue { DateTimeOffsets = [_dateTimeOffsetConverter.ToDateTimeOffset(member.CreateDate)] }, null, null),
            new IndexField(Constants.FieldNames.UpdateDate, new IndexValue { DateTimeOffsets = [_dateTimeOffsetConverter.ToDateTimeOffset(member.UpdateDate)] }, null, null),
            new IndexField(Constants.FieldNames.Name, new IndexValue { TextsR1 = [name], Keywords = [name] }, null, null),
            new IndexField(Constants.MemberFieldNames.Email, new IndexValue { TextsR2 = [member.Email], Keywords = [member.Email] }, null, null),
            new IndexField(Constants.MemberFieldNames.UserName, new IndexValue { TextsR2 = [member.UserName], Keywords = [member.UserName] }, null, null),
            new IndexField(Constants.MemberFieldNames.IsApproved, new IndexValue { Integers = [member.IsApproved ? 1 : 0] }, null, null),
            new IndexField(Constants.MemberFieldNames.IsLockedOut, new IndexValue { Integers = [member.IsLockedOut ? 1 : 0] }, null, null),
            new IndexField(Constants.MemberFieldNames.IsExternalMember, new IndexValue { Keywords = ["1"] }, null, null),
        ];
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

    private IContentBase? GetContent(ContentChange change)
        => change.ObjectType switch
        {
            UmbracoObjectTypes.Document => _contentService.GetById(change.Id),
            UmbracoObjectTypes.Media => _mediaService.GetById(change.Id),
            UmbracoObjectTypes.Member => _memberService.GetById(change.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(change), change.ObjectType, "This strategy only supports documents, media and members")
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
