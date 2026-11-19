using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Search;

/// <summary>
/// Refreshes the members of a member type when one of its existing properties is marked as sensitive, so that
/// the property's previously indexed values are removed from the members index on every server.
/// </summary>
/// <remarks>
/// Marking a property as sensitive isn't a structural change to the member type, so it doesn't otherwise cause
/// its members to be re-indexed.
/// </remarks>
internal sealed class MemberTypeSensitivePropertyIndexingNotificationHandler :
    INotificationHandler<MemberTypeSavingNotification>,
    INotificationHandler<MemberTypeSavedNotification>
{
    private const string NewlySensitivePropertyAliasesStateKey = "NewlySensitivePropertyAliases";

    private readonly IMemberTypeService _memberTypeService;
    private readonly IMemberService _memberService;
    private readonly DistributedCache _distributedCache;
    private readonly IOptionsMonitor<IndexingSettings> _indexingSettings;
    private readonly ILogger<MemberTypeSensitivePropertyIndexingNotificationHandler> _logger;

    public MemberTypeSensitivePropertyIndexingNotificationHandler(
        IMemberTypeService memberTypeService,
        IMemberService memberService,
        DistributedCache distributedCache,
        IOptionsMonitor<IndexingSettings> indexingSettings,
        ILogger<MemberTypeSensitivePropertyIndexingNotificationHandler> logger)
    {
        _memberTypeService = memberTypeService;
        _memberService = memberService;
        _distributedCache = distributedCache;
        _indexingSettings = indexingSettings;
        _logger = logger;
    }

    public void Handle(MemberTypeSavingNotification notification)
    {
        var newlySensitivePropertyAliases = new Dictionary<Guid, string[]>();
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

            var persistedAliases = persisted.PropertyTypes.Select(x => x.Alias).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var aliases = memberType.PropertyTypes
                .Select(propertyType => propertyType.Alias)
                .Where(alias => persistedAliases.Contains(alias)
                    && memberType.IsSensitiveProperty(alias)
                    && persisted.IsSensitiveProperty(alias) is false)
                .ToArray();

            if (aliases.Length > 0)
            {
                newlySensitivePropertyAliases[memberType.Key] = aliases;
            }
        }

        notification.State[NewlySensitivePropertyAliasesStateKey] = newlySensitivePropertyAliases;
    }

    public void Handle(MemberTypeSavedNotification notification)
    {
        if (notification.State.TryGetValue(NewlySensitivePropertyAliasesStateKey, out var value) is false
            || value is not Dictionary<Guid, string[]> newlySensitivePropertyAliases
            || newlySensitivePropertyAliases.Count == 0)
        {
            return;
        }

        var pageSize = _indexingSettings.CurrentValue.BatchSize;
        foreach (IMemberType memberType in notification.SavedEntities)
        {
            if (newlySensitivePropertyAliases.TryGetValue(memberType.Key, out var aliases) is false)
            {
                continue;
            }

            _logger.LogInformation(
                "Re-indexing members of member type {MemberTypeAlias} ({MemberTypeKey}) to remove the values of properties marked as sensitive: {PropertyTypeAliases}.",
                memberType.Alias,
                memberType.Key,
                aliases);

            var page = 0;
            var total = long.MaxValue;
            while (page * pageSize < total)
            {
                IMember[] members = _memberService.GetAll(
                    page,
                    pageSize,
                    out total,
                    "LoginName",
                    Direction.Ascending,
                    true,
                    memberType.Alias,
                    string.Empty).ToArray();

                if (members.Length > 0)
                {
                    _distributedCache.RefreshMemberCache(members);
                }

                page++;
            }
        }
    }
}
