// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

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
    private readonly IExternalMemberService _externalMemberService;
    private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalMemberIndexingNotificationHandler"/> class.
    /// </summary>
    public ExternalMemberIndexingNotificationHandler(
        IExternalMemberService externalMemberService,
        IUmbracoIndexingHandler umbracoIndexingHandler)
    {
        _externalMemberService = externalMemberService;
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
                _umbracoIndexingHandler.DeleteExternalMemberFromIndex(payload.Id);
            }
            else
            {
                ExternalMemberIdentity? member = _externalMemberService.GetByKeyAsync(payload.Key)
                    .GetAwaiter().GetResult();
                if (member is not null)
                {
                    _umbracoIndexingHandler.ReIndexForExternalMember(member);
                }
            }
        }
    }
}
