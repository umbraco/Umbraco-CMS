using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Search;

public sealed class MemberIndexingNotificationHandler : INotificationHandler<MemberCacheRefresherNotification>
{
    private readonly IMemberService _memberService;
    private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;

    public MemberIndexingNotificationHandler(
        IUmbracoIndexingHandler umbracoIndexingHandler,
        IMemberService memberService)
    {
        _umbracoIndexingHandler =
            umbracoIndexingHandler ?? throw new ArgumentNullException(nameof(umbracoIndexingHandler));
        _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
    }

    public void Handle(MemberCacheRefresherNotification args)
    {
        if (!_umbracoIndexingHandler.Enabled)
        {
            return;
        }

        if (Suspendable.ExamineEvents.CanIndex == false)
        {
            return;
        }

        switch (args.MessageType)
        {
            case MessageType.RefreshById:
                IMember? c1 = _memberService.GetById((int)args.MessageObject);
                if (c1 != null)
                {
                    _umbracoIndexingHandler.ReIndexForMember(c1);
                }

                break;
            case MessageType.RemoveById:

                // This is triggered when the item is permanently deleted
                _umbracoIndexingHandler.DeleteIndexForEntity((int)args.MessageObject, false);
                break;
            case MessageType.RefreshByInstance:
                if (args.MessageObject is IMember c3)
                {
                    _umbracoIndexingHandler.ReIndexForMember(c3);
                }

                break;
            case MessageType.RemoveByInstance:

                // This is triggered when the item is permanently deleted
                if (args.MessageObject is IMember c4)
                {
                    _umbracoIndexingHandler.DeleteIndexForEntity(c4.Id, false);
                }

                break;
            case MessageType.RefreshByPayload:
                var payload = (MemberCacheRefresher.JsonPayload[])args.MessageObject;
                foreach (MemberCacheRefresher.JsonPayload p in payload)
                {
                    if (p.Removed)
                    {
                        _umbracoIndexingHandler.DeleteIndexForEntity(p.Id, false);
                    }
                    else
                    {
                        IMember? m = _memberService.GetById(p.Id);
                        if (m != null)
                        {
                            _umbracoIndexingHandler.ReIndexForMember(m);
                        }
                    }
                }

                break;
            case MessageType.RefreshAll:
            case MessageType.RefreshByJson:
            default:
                // We don't support these, these message types will not fire for unpublished content
                break;
        }
    }
}
