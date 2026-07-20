using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services;

public interface IIndexer
{
    Task AddOrUpdateAsync(string indexAlias, Guid id, UmbracoObjectTypes objectType, IEnumerable<Variation> variations, IEnumerable<IndexField> fields, ContentProtection? protection);

    Task DeleteAsync(string indexAlias, IEnumerable<Guid> ids);

    Task ResetAsync(string indexAlias);

    Task<IndexMetadata> GetMetadataAsync(string indexAlias);
}
