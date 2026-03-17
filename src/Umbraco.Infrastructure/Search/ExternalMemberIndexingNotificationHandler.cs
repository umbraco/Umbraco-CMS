// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Search;

/// <summary>
///     Handles indexing of external-only members in response to cache refresher notifications.
/// </summary>
/// <remarks>
///     Like <see cref="MemberIndexingNotificationHandler"/>, this handler subscribes to a
///     <see cref="CacheRefresherNotification"/> rather than the domain notification directly.
///     This ensures Examine indexes are updated on all servers in a load-balanced environment.
/// </remarks>
public sealed class ExternalMemberIndexingNotificationHandler :
    INotificationHandler<ExternalMemberCacheRefresherNotification>
{
    private readonly IExamineManager _examineManager;
    private readonly IExternalMemberService _externalMemberService;
    private readonly IValueSetBuilder<ExternalMemberIdentity> _valueSetBuilder;
    private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalMemberIndexingNotificationHandler"/> class.
    /// </summary>
    public ExternalMemberIndexingNotificationHandler(
        IExamineManager examineManager,
        IExternalMemberService externalMemberService,
        IValueSetBuilder<ExternalMemberIdentity> valueSetBuilder,
        IUmbracoIndexingHandler umbracoIndexingHandler)
    {
        _examineManager = examineManager;
        _externalMemberService = externalMemberService;
        _valueSetBuilder = valueSetBuilder;
        _umbracoIndexingHandler = umbracoIndexingHandler;
    }

    /// <summary>
    ///     Handles the <see cref="ExternalMemberCacheRefresherNotification"/> by re-indexing
    ///     or removing external members from all registered member indexes.
    /// </summary>
    public void Handle(ExternalMemberCacheRefresherNotification notification)
    {
        if (!_umbracoIndexingHandler.Enabled)
        {
            return;
        }

        if (Suspendable.ExamineEvents.CanIndex == false)
        {
            return;
        }

        if (notification.MessageType != MessageType.RefreshByPayload)
        {
            return;
        }

        var payloads = (ExternalMemberCacheRefresher.JsonPayload[])notification.MessageObject;
        foreach (ExternalMemberCacheRefresher.JsonPayload payload in payloads)
        {
            if (payload.Removed)
            {
                foreach (IUmbracoMemberIndex index in _examineManager.Indexes.OfType<IUmbracoMemberIndex>())
                {
                    index.DeleteFromIndex(payload.Id.ToInvariantString());
                }
            }
            else
            {
                ExternalMemberIdentity? member = _externalMemberService.GetByKeyAsync(payload.Key)
                    .GetAwaiter().GetResult();
                if (member is not null)
                {
                    IEnumerable<ValueSet> valueSets = _valueSetBuilder.GetValueSets(member);
                    foreach (IUmbracoMemberIndex index in _examineManager.Indexes.OfType<IUmbracoMemberIndex>())
                    {
                        index.IndexItems(valueSets);
                    }
                }
            }
        }
    }
}
