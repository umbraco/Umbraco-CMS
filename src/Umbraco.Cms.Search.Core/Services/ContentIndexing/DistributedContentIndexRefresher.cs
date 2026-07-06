using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Cache.Content;
using Umbraco.Cms.Search.Core.Cache.Media;
using Umbraco.Cms.Search.Core.Cache.Member;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

internal sealed class DistributedContentIndexRefresher : IDistributedContentIndexRefresher
{
    private readonly DraftContentNotificationHandler _draftContentNotificationHandler;
    private readonly PublishedContentNotificationHandler _publishedContentNotificationHandler;
    private readonly DraftMediaNotificationHandler _draftMediaNotificationHandler;
    private readonly DraftMemberNotificationHandler _draftMemberNotificationHandler;

    public DistributedContentIndexRefresher(
        DraftContentNotificationHandler draftContentNotificationHandler,
        PublishedContentNotificationHandler publishedContentNotificationHandler,
        DraftMediaNotificationHandler draftMediaNotificationHandler,
        DraftMemberNotificationHandler draftMemberNotificationHandler)
    {
        _draftContentNotificationHandler = draftContentNotificationHandler;
        _publishedContentNotificationHandler = publishedContentNotificationHandler;
        _draftMediaNotificationHandler = draftMediaNotificationHandler;
        _draftMemberNotificationHandler = draftMemberNotificationHandler;
    }

    public void RefreshContent(IEnumerable<IContent> entities, ContentState contentState)
    {
        switch (contentState)
        {
            case ContentState.Draft:
                _draftContentNotificationHandler.Refresh(entities);
                break;
            case ContentState.Published:
                _publishedContentNotificationHandler.Refresh(entities);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(contentState), contentState, null);
        }
    }

    public void RefreshMedia(IEnumerable<IMedia> entities)
        => _draftMediaNotificationHandler.Refresh(entities);

    public void RefreshMember(IEnumerable<IMember> entities)
        => _draftMemberNotificationHandler.Refresh(entities);
}
