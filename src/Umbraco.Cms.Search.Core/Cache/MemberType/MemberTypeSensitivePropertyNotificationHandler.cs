using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.MemberType;

/// <summary>
/// Re-indexes the members of a member type when an existing property's sensitive flag changes.
/// </summary>
/// <remarks>
/// Changing whether a property is sensitive isn't a structural change to the member type, so on its own it doesn't
/// re-index the members. Without a re-index, values indexed before a property became sensitive would stay in the
/// index, and values of a property that is no longer sensitive would never be indexed.
/// </remarks>
internal sealed class MemberTypeSensitivePropertyNotificationHandler : ContentNotificationHandlerBase<MemberTypeCacheRefresher.JsonPayload>,
    INotificationHandler<MemberTypeSavingNotification>,
    INotificationHandler<MemberTypeSavedNotification>
{
    private const string ChangedSensitivePropertyAliasesStateKey = "ChangedSensitivePropertyAliases";

    private readonly IMemberTypeService _memberTypeService;
    private readonly ILogger<MemberTypeSensitivePropertyNotificationHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberTypeSensitivePropertyNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache used to broadcast the paired cache refresher notification.</param>
    /// <param name="originProvider">The provider of the current server origin.</param>
    /// <param name="indexDocumentService">The service used to flush the change-detection cache for affected documents.</param>
    /// <param name="memberTypeService">The service used to load the persisted member type for comparison.</param>
    /// <param name="logger">The logger.</param>
    public MemberTypeSensitivePropertyNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService,
        IMemberTypeService memberTypeService,
        ILogger<MemberTypeSensitivePropertyNotificationHandler> logger)
        : base(distributedCache, originProvider, indexDocumentService)
    {
        _memberTypeService = memberTypeService;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override Guid CacheRefresherUniqueId => MemberTypeCacheRefresher.UniqueId;

    /// <inheritdoc />
    public void Handle(MemberTypeSavingNotification notification)
    {
        var changedSensitivePropertyAliases = new Dictionary<Guid, string[]>();
        foreach (IMemberType memberType in notification.SavedEntities)
        {
            if (memberType.HasIdentity is false)
            {
                continue;
            }

            IMemberType? persisted = _memberTypeService.Get(memberType.Key);
            if (persisted is null)
            {
                continue;
            }

            // Properties added in this save have no indexed values yet, so only existing properties are compared.
            var persistedAliases = persisted.PropertyTypes.Select(x => x.Alias).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var aliases = memberType.PropertyTypes
                .Select(propertyType => propertyType.Alias)
                .Where(alias => persistedAliases.Contains(alias)
                    && memberType.IsSensitiveProperty(alias) != persisted.IsSensitiveProperty(alias))
                .ToArray();

            if (aliases.Length > 0)
            {
                changedSensitivePropertyAliases[memberType.Key] = aliases;
            }
        }

        notification.State[ChangedSensitivePropertyAliasesStateKey] = changedSensitivePropertyAliases;
    }

    /// <inheritdoc />
    public void Handle(MemberTypeSavedNotification notification)
    {
        if (notification.State.TryGetValue(ChangedSensitivePropertyAliasesStateKey, out var value) is false
            || value is not Dictionary<Guid, string[]> changedSensitivePropertyAliases
            || changedSensitivePropertyAliases.Count == 0)
        {
            return;
        }

        var payloads = new List<MemberTypeCacheRefresher.JsonPayload>();
        foreach (IMemberType memberType in notification.SavedEntities)
        {
            if (changedSensitivePropertyAliases.TryGetValue(memberType.Key, out var aliases) is false)
            {
                continue;
            }

            _logger.LogInformation(
                "Re-indexing members of member type {MemberTypeAlias} ({MemberTypeKey}) because the sensitive setting changed for properties: {PropertyTypeAliases}.",
                memberType.Alias,
                memberType.Key,
                aliases);

            payloads.Add(new MemberTypeCacheRefresher.JsonPayload(memberType.Key, ContentTypeChangeTypes.RefreshMain));
        }

        if (payloads.Count > 0)
        {
            HandlePayloads(payloads.ToArray());
        }
    }
}
