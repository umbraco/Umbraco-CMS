using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

public interface IContentIndexingDataCollectionService
{
    Task<IEnumerable<IndexField>?> CollectAsync(IContentBase content, bool published, CancellationToken cancellationToken);
}
