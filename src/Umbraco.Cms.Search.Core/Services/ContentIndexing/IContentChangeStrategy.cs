using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

public interface IContentChangeStrategy
{
    Task HandleAsync(IEnumerable<ContentIndexInfo> indexInfos, IEnumerable<ContentChange> changes, CancellationToken cancellationToken);

    Task RebuildAsync(ContentIndexInfo indexInfo, CancellationToken cancellationToken);
}
