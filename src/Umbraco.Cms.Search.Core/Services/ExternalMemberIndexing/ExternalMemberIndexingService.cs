using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Search.Configuration;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing.Indexers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.Services.ExternalMemberIndexing;

/// <summary>
/// Maintains external members in every registered content index that contains members.
/// </summary>
/// <remarks>
/// External members are not content - they have no content type, tree structure or properties - so they are indexed
/// independently of the content change strategies, as invariant member documents.
/// </remarks>
internal sealed class ExternalMemberIndexingService
{
    private const int PageSize = 1000;

    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IExternalMemberService _externalMemberService;
    private readonly IIndexerResolver _indexerResolver;
    private readonly IOriginProvider _originProvider;
    private readonly IEventAggregator _eventAggregator;
    private readonly IDateTimeOffsetConverter _dateTimeOffsetConverter;
    private readonly IndexOptions _indexOptions;
    private readonly ILogger<ExternalMemberIndexingService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalMemberIndexingService"/> class.
    /// </summary>
    /// <param name="backgroundTaskQueue">The queue used to run indexing work in the background.</param>
    /// <param name="externalMemberService">The service used to retrieve external members.</param>
    /// <param name="indexerResolver">The resolver used to obtain the indexer of each member index.</param>
    /// <param name="originProvider">The provider used to determine the current server's origin for same-origin filtering.</param>
    /// <param name="eventAggregator">The event aggregator used to publish the cancelable content indexing notification.</param>
    /// <param name="dateTimeOffsetConverter">The converter used to normalize indexed date values to <see cref="DateTimeOffset"/>.</param>
    /// <param name="indexOptions">The registered index configuration.</param>
    /// <param name="logger">The logger used to record rebuild cancellations.</param>
    public ExternalMemberIndexingService(
        IBackgroundTaskQueue backgroundTaskQueue,
        IExternalMemberService externalMemberService,
        IIndexerResolver indexerResolver,
        IOriginProvider originProvider,
        IEventAggregator eventAggregator,
        IDateTimeOffsetConverter dateTimeOffsetConverter,
        IOptions<IndexOptions> indexOptions,
        ILogger<ExternalMemberIndexingService> logger)
    {
        _backgroundTaskQueue = backgroundTaskQueue;
        _externalMemberService = externalMemberService;
        _indexerResolver = indexerResolver;
        _originProvider = originProvider;
        _eventAggregator = eventAggregator;
        _dateTimeOffsetConverter = dateTimeOffsetConverter;
        _indexOptions = indexOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Queues the given external member changes for indexing in all applicable member indexes.
    /// </summary>
    /// <param name="memberKeys">The keys of the changed external members.</param>
    /// <param name="removedMemberKeys">The keys of the removed external members.</param>
    /// <param name="origin">The server origin the changes were raised from.</param>
    public void Handle(Guid[] memberKeys, Guid[] removedMemberKeys, string origin)
    {
        var currentOrigin = _originProvider.GetCurrent();
        ContentIndexInfo[] indexInfos = _indexOptions
            .GetContentIndexRegistrations()
            .Where(registration => registration.SameOriginOnly is false || origin == currentOrigin)
            .Select(GetMemberIndexInfo)
            .WhereNotNull()
            .ToArray();

        if (indexInfos.Length is 0)
        {
            return;
        }

        _backgroundTaskQueue.QueueBackgroundWorkItem(async cancellationToken =>
        {
            ExternalMemberIdentity[] members = (await _externalMemberService.GetByKeysAsync(memberKeys)).ToArray();
            foreach (ExternalMemberIdentity member in members)
            {
                await UpdateIndexAsync(indexInfos, member);
            }

            var pendingRemovals = new List<Guid>(removedMemberKeys);
            pendingRemovals.AddRange(memberKeys.Except(members.Select(member => member.Key)));

            if (pendingRemovals.Count is 0)
            {
                return;
            }

            foreach (ContentIndexInfo indexInfo in indexInfos)
            {
                await indexInfo.Indexer.DeleteAsync(indexInfo.IndexAlias, pendingRemovals);
            }
        });
    }

    /// <summary>
    /// Adds all external members to the given index, if it contains members.
    /// </summary>
    /// <param name="indexAlias">The alias of the index being rebuilt.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task RebuildAsync(string indexAlias, CancellationToken cancellationToken)
    {
        ContentIndexRegistration? registration = _indexOptions.GetContentIndexRegistration(indexAlias);
        ContentIndexInfo? indexInfo = registration is not null ? GetMemberIndexInfo(registration) : null;
        if (indexInfo is null)
        {
            return;
        }

        ContentIndexInfo[] indexInfos = [indexInfo];
        var skip = 0;
        ExternalMemberIdentity[] members;
        do
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cancellation requested for indexing external members during rebuild of index: {indexAlias}", indexAlias);
                return;
            }

            members = (await _externalMemberService.GetAllAsync(skip, PageSize)).Items.ToArray();
            foreach (ExternalMemberIdentity member in members)
            {
                await UpdateIndexAsync(indexInfos, member);
            }

            skip += PageSize;
        }
        while (members.Length == PageSize);
    }

    private ContentIndexInfo? GetMemberIndexInfo(ContentIndexRegistration registration)
    {
        if (registration.ContainedObjectTypes.Contains(UmbracoObjectTypes.Member) is false)
        {
            return null;
        }

        IIndexer? indexer = _indexerResolver.GetIndexer(registration.IndexAlias);
        return indexer is not null
            ? new ContentIndexInfo(registration.IndexAlias, registration.ContainedObjectTypes, indexer)
            : null;
    }

    private async Task UpdateIndexAsync(ContentIndexInfo[] indexInfos, ExternalMemberIdentity member)
    {
        IndexField[] fields = GetIndexFields(member).ToArray();
        Variation[] variations = [new(null, null)];

        foreach (ContentIndexInfo indexInfo in indexInfos)
        {
            var notification = new ContentIndexingNotification(indexInfo.IndexAlias, member.Key, UmbracoObjectTypes.Member, variations, fields);
            if (await _eventAggregator.PublishCancelableAsync(notification))
            {
                continue;
            }

            await indexInfo.Indexer.AddOrUpdateAsync(indexInfo.IndexAlias, member.Key, UmbracoObjectTypes.Member, variations, notification.Fields, null);
        }
    }

    private IEnumerable<IndexField> GetIndexFields(ExternalMemberIdentity member)
    {
        var name = member.Name.IsNullOrWhiteSpace() ? member.UserName : member.Name!;

        IndexField[] systemFields =
        [
            new(Umbraco.Cms.Core.Constants.Search.FieldNames.Id, new() { Keywords = [member.Key.AsKeyword()] }, null, null),
            new(Umbraco.Cms.Core.Constants.Search.FieldNames.PathIds, new() { Keywords = [member.Key.AsKeyword()] }, null, null),
            new(Umbraco.Cms.Core.Constants.Search.FieldNames.ObjectType, new() { Keywords = [nameof(UmbracoObjectTypes.Member)] }, null, null),
            new(Umbraco.Cms.Core.Constants.Search.FieldNames.CreateDate, new() { DateTimeOffsets = [_dateTimeOffsetConverter.ToDateTimeOffset(member.CreateDate)] }, null, null),
            new(Umbraco.Cms.Core.Constants.Search.FieldNames.UpdateDate, new() { DateTimeOffsets = [_dateTimeOffsetConverter.ToDateTimeOffset(member.UpdateDate)] }, null, null),
            new(Umbraco.Cms.Core.Constants.Search.FieldNames.Name, new() { TextsR1 = [name], Keywords = [name] }, null, null),
        ];

        IndexField[] memberFields = systemFields
            .Concat(MemberIndexFields.Create(member.Email, member.UserName, member.IsApproved, member.IsLockedOut, isExternalMember: true))
            .ToArray();

        return memberFields.Concat(
            ExternalMemberProfileDataIndexFields.Create(member.ProfileData, memberFields.Select(field => field.FieldName)));
    }
}
