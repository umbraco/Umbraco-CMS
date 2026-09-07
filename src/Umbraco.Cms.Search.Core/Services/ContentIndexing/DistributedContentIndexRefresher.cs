using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Cache.Content;
using Umbraco.Cms.Search.Core.Cache.Media;
using Umbraco.Cms.Search.Core.Cache.Member;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Default implementation of <see cref="IDistributedContentIndexRefresher"/>, backed by the search cache refreshers.
/// </summary>
internal sealed class DistributedContentIndexRefresher : IDistributedContentIndexRefresher
{
    private readonly DraftContentNotificationHandler _draftContentNotificationHandler;
    private readonly PublishedContentNotificationHandler _publishedContentNotificationHandler;
    private readonly DraftMediaNotificationHandler _draftMediaNotificationHandler;
    private readonly DraftMemberNotificationHandler _draftMemberNotificationHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedContentIndexRefresher"/> class.
    /// </summary>
    /// <param name="draftContentNotificationHandler">The handler used to broadcast draft content changes to all servers.</param>
    /// <param name="publishedContentNotificationHandler">The handler used to broadcast published content changes to all servers.</param>
    /// <param name="draftMediaNotificationHandler">The handler used to broadcast media changes to all servers.</param>
    /// <param name="draftMemberNotificationHandler">The handler used to broadcast member changes to all servers.</param>
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void RefreshMedia(IEnumerable<IMedia> entities)
        => _draftMediaNotificationHandler.Refresh(entities);

    /// <inheritdoc />
    public void RefreshMember(IEnumerable<IMember> entities)
        => _draftMemberNotificationHandler.Refresh(entities);
}
