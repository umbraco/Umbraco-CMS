using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Notifies every server in a load-balanced farm that content, media or members changed, so each can re-index them.
/// </summary>
public interface IDistributedContentIndexRefresher
{
    /// <summary>
    /// Notifies all servers that the given documents changed.
    /// </summary>
    /// <param name="entities">The changed documents.</param>
    /// <param name="contentState">Whether the change concerns the draft or published version.</param>
    void RefreshContent(IEnumerable<IContent> entities, ContentState contentState);

    /// <summary>
    /// Notifies all servers that the given media items changed.
    /// </summary>
    /// <param name="entities">The changed media items.</param>
    void RefreshMedia(IEnumerable<IMedia> entities);

    /// <summary>
    /// Notifies all servers that the given members changed.
    /// </summary>
    /// <param name="entities">The changed members.</param>
    void RefreshMember(IEnumerable<IMember> entities);
}
