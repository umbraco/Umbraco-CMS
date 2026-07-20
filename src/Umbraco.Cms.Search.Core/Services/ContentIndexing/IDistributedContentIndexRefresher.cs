using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

public interface IDistributedContentIndexRefresher
{
    void RefreshContent(IEnumerable<IContent> entities, ContentState contentState);

    void RefreshMedia(IEnumerable<IMedia> entities);

    void RefreshMember(IEnumerable<IMember> entities);
}
